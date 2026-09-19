using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI.Tests
{
    public class LayoutRegressionTests
    {
        private sealed class TestLayoutItem : LayoutItem
        {
            public void ApplyFloatingForTest() => ApplyFloatingLayout();
        }

        [Test]
        public void FloatingExpand_IsStableAcrossRepeatedLayoutPasses() {
            GameObject parent = new("Parent", typeof(RectTransform));
            try {
                parent.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                GameObject child = new("Child", typeof(RectTransform), typeof(TestLayoutItem));
                child.transform.SetParent(parent.transform, false);

                RectTransform rect = child.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(20, 20);
                TestLayoutItem item = child.GetComponent<TestLayoutItem>();
                item.IsFloating = true;
                item.AttachTo = LayoutItem.FloatingAttachTo.Parent;
                item.FloatingExpand = new Vector2(2, 3);

                item.ApplyFloatingForTest();
                Vector2 firstSize = rect.rect.size;
                item.ApplyFloatingForTest();

                Assert.That(firstSize, Is.EqualTo(new Vector2(24, 26)));
                Assert.That(rect.rect.size, Is.EqualTo(firstSize));

                item.MaxSize = new Vector2(22, 22);
                item.ApplyFloatingForTest();
                Assert.That(rect.rect.size, Is.EqualTo(new Vector2(22, 22)));

                item.MaxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                item.ApplyFloatingForTest();
                Assert.That(rect.rect.size, Is.EqualTo(firstSize));
            }
            finally {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void Layout_RebuildRefreshesDirectChildMembershipWithoutManualCacheRefresh() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 40);
                Layout layout = root.GetComponent<Layout>();

                GameObject first = new("First", typeof(RectTransform));
                first.transform.SetParent(root.transform, false);
                first.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                Assert.That(layout.ChildCount, Is.EqualTo(1));

                Object.DestroyImmediate(first);
                GameObject second = new("Second", typeof(RectTransform));
                second.transform.SetParent(root.transform, false);
                second.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 20);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(layout.ChildCount, Is.EqualTo(1));
                Assert.That(second.GetComponent<RectTransform>().rect.width, Is.EqualTo(30).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_ChildConfigurationChangeIsObservedOnNextRebuild() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 40);
                Layout layout = root.GetComponent<Layout>();
                GameObject child = new("Child", typeof(RectTransform), typeof(LayoutItem));
                child.transform.SetParent(root.transform, false);
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.sizeDelta = new Vector2(20, 20);
                LayoutItem item = child.GetComponent<LayoutItem>();

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                item.Margin = new Margins { left = 10 };
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(childRect.anchoredPosition.x, Is.EqualTo(10).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_FixedTracks_ClampsCounts_AndFillGrowChildren() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 0, minTrackSize = 120, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 0, minTrackSize = 120, fillExtraSpace = 1 };

                GameObject active = new("Active", typeof(RectTransform), typeof(LayoutItem));
                active.transform.SetParent(root.transform, false);
                LayoutItem item = active.GetComponent<LayoutItem>();
                item.SizeMode = new LayoutItem.SizeModes { x = SizingMode.Grow, y = SizingMode.Grow };
                item.Margin = new Margins { left = 5, right = 7, top = 3, bottom = 2 };

                GameObject inactive = new("Inactive", typeof(RectTransform));
                inactive.transform.SetParent(root.transform, false);
                inactive.SetActive(false);

                GameObject floating = new("Floating", typeof(RectTransform), typeof(LayoutItem));
                floating.transform.SetParent(root.transform, false);
                floating.GetComponent<LayoutItem>().IsFloating = true;

                GameObject ignored = new("Ignored", typeof(RectTransform), typeof(LayoutElement));
                ignored.transform.SetParent(root.transform, false);
                ignored.GetComponent<LayoutElement>().ignoreLayout = true;

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                // Track counts of zero clamp to one.
                Assert.That(grid.ActiveChildCount, Is.EqualTo(1));
                Assert.That(grid.ResolvedRows, Is.EqualTo(1));
                Assert.That(grid.ResolvedColumns, Is.EqualTo(1));
                Assert.That(grid.ColumnWidth, Is.EqualTo(100));
                Assert.That(grid.RowHeight, Is.EqualTo(50));
                // Grow children fill the track minus their own margins.
                Assert.That(active.GetComponent<RectTransform>().rect.size, Is.EqualTo(new Vector2(88, 45)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_SerializedUniformValue_MigratesToAutoFit() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                GridLayout grid = root.GetComponent<GridLayout>();
                SerializedObject serialized = new(grid);
                SerializedProperty mode = serialized.FindProperty("m_columnTracks").FindPropertyRelative("mode");
                mode.enumValueIndex = 3; // Uniform in the removed grid API.
                serialized.ApplyModifiedPropertiesWithoutUndo();

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                Assert.That(grid.ColumnTracks.mode, Is.EqualTo(GridTrackMode.AutoFit));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_InspectorContract_AllowsMinimumAndFillExtraSpaceForFixedCount() {
            GameObject first = new("GridA", typeof(RectTransform), typeof(GridLayout));
            GameObject second = new("GridB", typeof(RectTransform), typeof(GridLayout));
            try {
                GridLayout firstGrid = first.GetComponent<GridLayout>();
                GridLayout secondGrid = second.GetComponent<GridLayout>();
                SerializedObject serialized = new(new Object[] { firstGrid, secondGrid });
                SerializedProperty tracks = serialized.FindProperty("m_columnTracks");
                tracks.FindPropertyRelative("mode").enumValueIndex = (int)GridTrackMode.FixedCount;
                tracks.FindPropertyRelative("minTrackSize").floatValue = 64;
                tracks.FindPropertyRelative("fillExtraSpace").floatValue = 2;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(firstGrid.ColumnTracks.minTrackSize, Is.EqualTo(64));
                Assert.That(secondGrid.ColumnTracks.minTrackSize, Is.EqualTo(64));
                Assert.That(firstGrid.ColumnTracks.fillExtraSpace, Is.EqualTo(2));
                Assert.That(secondGrid.ColumnTracks.fillExtraSpace, Is.EqualTo(2));
            }
            finally {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void GridLayout_ZeroFillExtraSpace_KeepsMinimumTrackSize() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                root.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 40, fillExtraSpace = 0 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 1, minTrackSize = 20, fillExtraSpace = 0 };

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                Assert.That(grid.ColumnWidth, Is.EqualTo(40));
                Assert.That(grid.RowHeight, Is.EqualTo(20));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_MinimumTracks_DoNotShrinkWhenContainerIsTooSmall() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 80, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 1, minTrackSize = 20, fillExtraSpace = 1 };

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                Assert.That(grid.ColumnWidth, Is.EqualTo(80));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_AutoFill_FitContentUsesFiniteChildBound() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.SizeMode = new LayoutItem.SizeModes { x = SizingMode.FitContent, y = SizingMode.Fixed };
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.AutoFill, count = 1, minTrackSize = 40, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 1, minTrackSize = 20, fillExtraSpace = 1 };

                for(int i = 0; i < 2; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                Assert.That(grid.ResolvedColumns, Is.EqualTo(2));
                Assert.That(grid.ColumnWidth, Is.EqualTo(40));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_AppliesGapsMarginsAndPercentSizingInsideCells() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(220, 120);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 40, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 20, fillExtraSpace = 1 };
                grid.Gap = new Vector2(10, 5);

                GameObject child = new("Percent", typeof(RectTransform), typeof(LayoutItem));
                child.transform.SetParent(root.transform, false);
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                childRect.pivot = new Vector2(0, 1);
                LayoutItem item = child.GetComponent<LayoutItem>();
                item.SizeMode = new LayoutItem.SizeModes { x = SizingMode.Percent, y = SizingMode.Percent };
                item.Percentage = new Vector2(0.5f, 0.5f);
                item.Margin = new Margins { left = 5, right = 5, top = 2, bottom = 3 };

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                // Tracks are (220 - 10) / 2 = 105 and (120 - 5) / 2 = 57.5.
                Assert.That(child.GetComponent<RectTransform>().rect.size.x, Is.EqualTo(47.5f).Within(0.01f));
                Assert.That(child.GetComponent<RectTransform>().rect.size.y, Is.EqualTo(26.25f).Within(0.01f));
                Assert.That(child.GetComponent<RectTransform>().anchoredPosition, Is.EqualTo(new Vector2(5, -2)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_CentersFixedChildInsideItsCell() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 50);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 1, minTrackSize = 0, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 1, minTrackSize = 0, fillExtraSpace = 1 };
                grid.ChildAlignmentX = Layout.Alignment.Center;
                grid.ChildAlignmentY = Layout.Alignment.Center;

                GameObject child = new("Child", typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                childRect.pivot = new Vector2(0, 1);
                childRect.sizeDelta = new Vector2(20, 10);

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(childRect.anchoredPosition, Is.EqualTo(new Vector2(40, -20)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_FixedColumns_RemainAtTheirDeclaredCountWhenFilled() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 100);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 40, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 20, fillExtraSpace = 1 };

                for(int i = 0; i < 4; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(grid.ResolvedColumns, Is.EqualTo(2));
                Assert.That(grid.ResolvedRows, Is.EqualTo(2));
                Assert.That(grid.ColumnWidth, Is.EqualTo(100).Within(0.01f));
                Assert.That(grid.RowHeight, Is.EqualTo(50).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_FixedRows_ExtendForOverflowingChildFlow() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 100);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 40, fillExtraSpace = 1 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 20, fillExtraSpace = 1 };

                for(int i = 0; i < 5; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                }

                Assert.DoesNotThrow(() => LayoutRebuilder.ForceRebuildLayoutImmediate(rect));
                Assert.That(grid.ResolvedColumns, Is.EqualTo(2));
                Assert.That(grid.ResolvedRows, Is.EqualTo(3));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_RefreshesWhenPlainChildIsDisabled() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                GridLayout grid = root.GetComponent<GridLayout>();
                GameObject child = new("Child", typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());
                Assert.That(grid.ActiveChildCount, Is.EqualTo(1));

                child.SetActive(false);
                grid.Update();

                Assert.That(grid.ActiveChildCount, Is.EqualTo(0));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GridLayout_AutoFitTracks_ExpandRowsForChildCount() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                root.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 100);
                GridLayout grid = root.GetComponent<GridLayout>();

                for(int i = 0; i < 5; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform), typeof(LayoutItem));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 30);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                // Default tracks: AutoFit columns (packing tracks of at least minTrackSize,
                // capped by the item count) and one FixedCount row.
                // 360px fits exactly three 120px tracks, so five items resolve to
                // three columns and two rows.
                Assert.That(grid.ResolvedColumns, Is.EqualTo(3));
                Assert.That(grid.ResolvedRows, Is.EqualTo(2));
                Assert.That(grid.ColumnWidth, Is.EqualTo(120));
                Assert.That(grid.RowHeight, Is.EqualTo(50));

                RectTransform[] children = new RectTransform[5];
                for(int i = 0; i < 5; i++)
                    children[i] = root.transform.GetChild(i).GetComponent<RectTransform>();

                // Fixed children keep their own size inside their track.
                Assert.That(children[0].rect.size, Is.EqualTo(new Vector2(40, 30)));
                Assert.That(children[0].anchoredPosition, Is.EqualTo(new Vector2(0, 0)));
                Assert.That(children[1].anchoredPosition, Is.EqualTo(new Vector2(120, 0)));
                Assert.That(children[2].anchoredPosition, Is.EqualTo(new Vector2(240, 0)));
                Assert.That(children[3].anchoredPosition, Is.EqualTo(new Vector2(0, -50)));
                Assert.That(children[4].anchoredPosition, Is.EqualTo(new Vector2(120, -50)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_DefaultNoWrap_DoesNotBreakOverflowingRow() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);

                RectTransform[] items = new RectTransform[4];
                for(int i = 0; i < 4; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(root.GetComponent<Layout>().Wrap, Is.EqualTo(Layout.FlexWrap.NoWrap));
                Assert.That(items[0].anchoredPosition, Is.EqualTo(new Vector2(0, 0)));
                Assert.That(items[1].anchoredPosition, Is.EqualTo(new Vector2(40, 0)));
                Assert.That(items[2].anchoredPosition, Is.EqualTo(new Vector2(80, 0)));
                Assert.That(items[3].anchoredPosition, Is.EqualTo(new Vector2(120, 0)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_Wrap_BreaksLines_AndPlacesWrappedItems() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);
                Layout layout = root.GetComponent<Layout>();
                layout.Wrap = Layout.FlexWrap.Wrap;

                RectTransform[] items = new RectTransform[4];
                for(int i = 0; i < 4; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[0].anchoredPosition, Is.EqualTo(new Vector2(0, 0)));
                Assert.That(items[1].anchoredPosition, Is.EqualTo(new Vector2(40, 0)));
                Assert.That(items[2].anchoredPosition, Is.EqualTo(new Vector2(0, -20)));
                Assert.That(items[3].anchoredPosition, Is.EqualTo(new Vector2(40, -20)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_Wrap_UsesCrossAxisGapBetweenLines() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);
                Layout layout = root.GetComponent<Layout>();
                layout.Wrap = Layout.FlexWrap.Wrap;
                layout.CrossAxisGap = 5;

                RectTransform[] items = new RectTransform[3];
                for(int i = 0; i < 3; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[2].anchoredPosition.y, Is.EqualTo(-25).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_Wrap_BreaksLines_AndPlacesWrappedItems() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Wrap = Layout.FlexWrap.Wrap;

                RectTransform[] items = new RectTransform[4];
                for(int i = 0; i < 4; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                // Two items per line: line two flows below line one.
                Assert.That(items[0].anchoredPosition, Is.EqualTo(new Vector2(0, 0)));
                Assert.That(items[1].anchoredPosition, Is.EqualTo(new Vector2(40, 0)));
                Assert.That(items[2].anchoredPosition, Is.EqualTo(new Vector2(0, -20)));
                Assert.That(items[3].anchoredPosition, Is.EqualTo(new Vector2(40, -20)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_Wrap_SpaceBetween_DistributesEachLineIndependently() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 50);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Wrap = Layout.FlexWrap.Wrap;
                SerializedObject serialized = new(flex);
                serialized.FindProperty("m_justifyContent").enumValueIndex = (int)Layout.Justification.SpaceBetween;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                RectTransform[] items = new RectTransform[4];
                for(int i = 0; i < items.Length; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(20, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[0].anchoredPosition.x, Is.EqualTo(0).Within(0.01f));
                Assert.That(items[1].anchoredPosition.x, Is.EqualTo(80).Within(0.01f));
                Assert.That(items[2].anchoredPosition.x, Is.EqualTo(0).Within(0.01f));
                Assert.That(items[3].anchoredPosition.x, Is.EqualTo(80).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_Wrap_ConstrainedGrow_RedistributesRemainingLineSpace() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 40);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Wrap = Layout.FlexWrap.Wrap;

                GameObject constrained = new("Constrained", typeof(RectTransform), typeof(LayoutItem));
                constrained.transform.SetParent(root.transform, false);
                RectTransform constrainedRect = constrained.GetComponent<RectTransform>();
                constrainedRect.anchorMin = constrainedRect.anchorMax = new Vector2(0, 1);
                constrainedRect.pivot = new Vector2(0, 1);
                LayoutItem constrainedItem = constrained.GetComponent<LayoutItem>();
                constrainedItem.SizeMode = new LayoutItem.SizeModes { x = SizingMode.Grow, y = SizingMode.Fixed };
                constrainedItem.MaxSize = new Vector2(30, float.PositiveInfinity);

                GameObject flexible = new("Flexible", typeof(RectTransform), typeof(LayoutItem));
                flexible.transform.SetParent(root.transform, false);
                RectTransform flexibleRect = flexible.GetComponent<RectTransform>();
                flexibleRect.anchorMin = flexibleRect.anchorMax = new Vector2(0, 1);
                flexibleRect.pivot = new Vector2(0, 1);
                LayoutItem flexibleItem = flexible.GetComponent<LayoutItem>();
                flexibleItem.SizeMode = new LayoutItem.SizeModes { x = SizingMode.Grow, y = SizingMode.Fixed };

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(constrainedRect.rect.width, Is.EqualTo(30).Within(0.01f));
                Assert.That(flexibleRect.rect.width, Is.EqualTo(70).Within(0.01f));
                Assert.That(flexibleRect.anchoredPosition.x, Is.EqualTo(30).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_Reverse_ChangesPrimaryFlowAtRuntime() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 40);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Reverse = true;

                RectTransform[] items = new RectTransform[2];
                for(int i = 0; i < items.Length; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(20, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[0].anchoredPosition.x, Is.EqualTo(-20).Within(0.01f));
                Assert.That(items[1].anchoredPosition.x, Is.EqualTo(-40).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexColumn_Reverse_ChangesPrimaryFlowAtRuntime() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexColumn));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(40, 100);
                FlexColumn flex = root.GetComponent<FlexColumn>();
                flex.Reverse = true;

                RectTransform[] items = new RectTransform[2];
                for(int i = 0; i < items.Length; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(20, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[0].anchoredPosition.y, Is.EqualTo(20).Within(0.01f));
                Assert.That(items[1].anchoredPosition.y, Is.EqualTo(40).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_WrapReverse_PlacesFirstLineAtFarCrossEdge() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Wrap = Layout.FlexWrap.WrapReverse;

                RectTransform[] items = new RectTransform[4];
                for(int i = 0; i < items.Length; i++) {
                    GameObject child = new($"Child{i}", typeof(RectTransform));
                    child.transform.SetParent(root.transform, false);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    childRect.anchorMin = childRect.anchorMax = new Vector2(0, 1);
                    childRect.pivot = new Vector2(0, 1);
                    childRect.sizeDelta = new Vector2(40, 20);
                    items[i] = childRect;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(items[0].anchoredPosition.y, Is.EqualTo(-80).Within(0.01f));
                Assert.That(items[2].anchoredPosition.y, Is.EqualTo(-60).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlexRow_Wrap_ExcludesIgnoredFloatingChildren_AndAppliesGrowMargins() {
            GameObject root = new("Flex", typeof(RectTransform), typeof(FlexRow));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 40);
                FlexRow flex = root.GetComponent<FlexRow>();
                flex.Wrap = Layout.FlexWrap.Wrap;

                GameObject active = new("Active", typeof(RectTransform), typeof(LayoutItem));
                active.transform.SetParent(root.transform, false);
                RectTransform activeRect = active.GetComponent<RectTransform>();
                activeRect.anchorMin = activeRect.anchorMax = new Vector2(0, 1);
                activeRect.pivot = new Vector2(0, 1);
                activeRect.sizeDelta = new Vector2(10, 20);
                LayoutItem activeItem = active.GetComponent<LayoutItem>();
                activeItem.SizeMode = new LayoutItem.SizeModes { x = SizingMode.Grow, y = SizingMode.Fixed };
                activeItem.Margin = new Margins { left = 5, right = 7 };

                GameObject floating = new("Floating", typeof(RectTransform), typeof(LayoutItem));
                floating.transform.SetParent(root.transform, false);
                floating.GetComponent<LayoutItem>().IsFloating = true;

                GameObject ignored = new("Ignored", typeof(RectTransform), typeof(LayoutElement));
                ignored.transform.SetParent(root.transform, false);
                ignored.GetComponent<LayoutElement>().ignoreLayout = true;

                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

                Assert.That(activeRect.rect.width, Is.EqualTo(88).Within(0.01f));
                Assert.That(activeRect.anchoredPosition.x, Is.EqualTo(5).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SpaceBetween_DoesNotUseNegativeSpacingWhenContentOverflows() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
                Layout layout = root.GetComponent<Layout>();
                SerializedObject serialized = new(layout);
                serialized.FindProperty("m_justifyContent").enumValueIndex = (int)Layout.Justification.SpaceBetween;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                GameObject first = new("First", typeof(RectTransform));
                first.transform.SetParent(root.transform, false);
                first.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 20);
                GameObject second = new("Second", typeof(RectTransform));
                second.transform.SetParent(root.transform, false);
                second.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 20);

                layout.RefreshChildCache();
                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                float distance = second.GetComponent<RectTransform>().anchoredPosition.x - first.GetComponent<RectTransform>().anchoredPosition.x;
                Assert.That(distance, Is.EqualTo(80).Within(0.01f));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }





        [Test]
        public void CircularSizingDetection_FindsOnlyParentDependentAxesInNormalFlow() {
            GameObject parentObject = new("Parent", typeof(RectTransform), typeof(Layout));
            try {
                Layout parent = parentObject.GetComponent<Layout>();
                parent.SizeMode = new LayoutItem.SizeModes {
                    x = SizingMode.FitContent,
                    y = SizingMode.FitContent
                };

                GameObject childObject = new("Child", typeof(RectTransform), typeof(LayoutItem));
                childObject.transform.SetParent(parentObject.transform, false);
                LayoutItem child = childObject.GetComponent<LayoutItem>();
                LayoutItem.SizeModes childSizing = new() {
                    x = SizingMode.Grow,
                    y = SizingMode.Fixed
                };

                Assert.That(
                    LayoutUtil.FindCircularSizingAxes(child, childSizing, childIsFloating: false),
                    Is.EqualTo(LayoutAxis.Horizontal)
                );

                childSizing.y = SizingMode.Percent;
                Assert.That(
                    LayoutUtil.FindCircularSizingAxes(child, childSizing, childIsFloating: false),
                    Is.EqualTo(LayoutAxis.Horizontal | LayoutAxis.Vertical)
                );

                Assert.That(
                    LayoutUtil.FindCircularSizingAxes(child, childSizing, childIsFloating: true),
                    Is.EqualTo(LayoutAxis.None)
                );
            }
            finally {
                Object.DestroyImmediate(parentObject);
            }
        }

        [Test]
        public void Layout_PublishesPreferredMeasurementWithoutPositioningDuringInput() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(200, 80);
                Layout layout = root.GetComponent<Layout>();
                layout.SizeMode = new LayoutItem.SizeModes { x = SizingMode.FitContent, y = SizingMode.FitContent };

                GameObject child = new("Child", typeof(RectTransform), typeof(LayoutItem));
                child.transform.SetParent(root.transform, false);
                RectTransform childRect = child.GetComponent<RectTransform>();
                childRect.sizeDelta = new Vector2(40, 20);
                child.GetComponent<LayoutItem>().Margin = new Margins { left = 5, right = 7, top = 3, bottom = 4 };
                childRect.anchoredPosition = new Vector2(17, -11);

                layout.CalculateLayoutInputHorizontal();
                layout.CalculateLayoutInputVertical();

                Assert.That(layout.minWidth, Is.GreaterThanOrEqualTo(0));
                Assert.That(layout.preferredWidth, Is.EqualTo(52).Within(0.01f));
                Assert.That(layout.preferredHeight, Is.EqualTo(27).Within(0.01f));
                Assert.That(childRect.anchoredPosition, Is.EqualTo(new Vector2(17, -11)));
                Assert.That(rootRect.rect.size, Is.EqualTo(new Vector2(200, 80)));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Grid_PublishesTrackMeasurementIncludingPaddingAndGaps() {
            GameObject root = new("Grid", typeof(RectTransform), typeof(GridLayout));
            try {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 100);
                GridLayout grid = root.GetComponent<GridLayout>();
                grid.Padding = new Margins { left = 4, right = 6, top = 3, bottom = 5 };
                grid.Gap = new Vector2(8, 10);
                grid.ColumnTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 30, fillExtraSpace = 0 };
                grid.RowTracks = new GridTrackDef { mode = GridTrackMode.FixedCount, count = 2, minTrackSize = 20, fillExtraSpace = 0 };

                grid.CalculateLayoutInputHorizontal();
                grid.CalculateLayoutInputVertical();

                Assert.That(grid.minWidth, Is.EqualTo(78).Within(0.01f));
                Assert.That(grid.minHeight, Is.EqualTo(58).Within(0.01f));
                Assert.That(grid.preferredWidth, Is.GreaterThanOrEqualTo(grid.minWidth));
                Assert.That(grid.preferredHeight, Is.GreaterThanOrEqualTo(grid.minHeight));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_DoesNotModifyExistingScrollRectOwnership() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(ScrollRect), typeof(Layout));
            try {
                ScrollRect scroll = root.GetComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.inertia = false;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());

                Assert.That(scroll.horizontal, Is.False);
                Assert.That(scroll.vertical, Is.True);
                Assert.That(scroll.inertia, Is.False);
                Assert.That(scroll.movementType, Is.EqualTo(ScrollRect.MovementType.Clamped));
                Assert.That(root.GetComponent<RectMask2D>(), Is.Null);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Layout_RefreshChildCache_SkipsNonRectTransformChildren() {
            GameObject root = new("Layout", typeof(RectTransform), typeof(Layout));
            try {
                GameObject child = new("NonUIChild");
                child.transform.SetParent(root.transform, false);
                Layout layout = root.GetComponent<Layout>();

                Assert.DoesNotThrow(layout.RefreshChildCache);
                Assert.That(layout.ChildCount, Is.EqualTo(0));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }
    }
}
