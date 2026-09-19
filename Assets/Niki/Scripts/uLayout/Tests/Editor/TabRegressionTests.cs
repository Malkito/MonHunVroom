using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI.Tests
{
    public class TabRegressionTests
    {
        [Test]
        public void Select_UsesExplicitEntryMapping_AndDoesNotRepeatEvents() {
            GameObject root = CreateGroup(out TabGroup group);
            try {
                GameObject decoration = new("Decoration", typeof(RectTransform));
                decoration.transform.SetParent(root.transform, false);
                TabButton first = CreateButton(root, "First");
                TabButton second = CreateButton(root, "Second");
                GameObject firstPage = new("FirstPage");
                GameObject secondPage = new("SecondPage");
                firstPage.transform.SetParent(root.transform, false);
                secondPage.transform.SetParent(root.transform, false);

                group.Register(first, firstPage);
                group.Register(second, secondPage);
                group.Register(first, firstPage);
                Assert.That(group.Tabs.Count, Is.EqualTo(2));
                first.transform.SetAsLastSibling();

                int firstSelected = 0;
                int firstDeselected = 0;
                int secondSelected = 0;
                first.OnSelected.AddListener(() => firstSelected++);
                first.OnDeselected.AddListener(() => firstDeselected++);
                second.OnSelected.AddListener(() => secondSelected++);

                Assert.That(group.Select(first), Is.True);
                Assert.That(group.Select(second), Is.True);
                Assert.That(group.Select(second), Is.True);

                Assert.That(group.SelectedIndex, Is.EqualTo(1));
                Assert.That(group.SelectedButton, Is.EqualTo(second));
                Assert.That(group.SelectedPage, Is.EqualTo(secondPage));
                Assert.That(firstPage.activeSelf, Is.False);
                Assert.That(secondPage.activeSelf, Is.True);
                Assert.That(firstSelected, Is.EqualTo(1));
                Assert.That(firstDeselected, Is.EqualTo(1));
                Assert.That(secondSelected, Is.EqualTo(1));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void OnEnable_SelectsConfiguredInitialTab() {
            GameObject root = new("TabGroup", typeof(RectTransform));
            root.SetActive(false);
            try {
                ToggleGroup toggleGroup = root.AddComponent<ToggleGroup>();
                TabGroup group = root.AddComponent<TabGroup>();
                TabButton first = CreateButton(root, "First");
                TabButton second = CreateButton(root, "Second");
                GameObject firstPage = new("FirstPage");
                GameObject secondPage = new("SecondPage");
                firstPage.transform.SetParent(root.transform, false);
                secondPage.transform.SetParent(root.transform, false);
                group.Register(first, firstPage);
                group.Register(second, secondPage);
                group.InitialIndex = 1;
                group.AllowNoSelection = false;

                root.SetActive(true);
                group.InitializeSelection(); // EditMode does not run the normal player Start loop.

                Assert.That(group.ToggleGroup, Is.EqualTo(toggleGroup));
                Assert.That(group.SelectedIndex, Is.EqualTo(1));
                Assert.That(firstPage.activeSelf, Is.False);
                Assert.That(secondPage.activeSelf, Is.True);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Toggle_UsesBuiltInSelectionAndAllowsOptionalGraphics() {
            GameObject root = CreateGroup(out TabGroup group);
            try {
                TabButton first = CreateButton(root, "First", addImage: false);
                TabButton second = CreateButton(root, "Second", addImage: false);
                group.Register(first);
                group.Register(second);
                group.Select(first);

                second.Toggle.isOn = true;

                Assert.That(group.SelectedButton, Is.EqualTo(second));
                Assert.That(second.Toggle.group, Is.EqualTo(group.ToggleGroup));
                Assert.That(first.Toggle.isOn, Is.False);
                Assert.That(second.Toggle.isOn, Is.True);
                Assert.That(first.TargetGraphic, Is.Null);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Navigation_SkipsNonInteractableTabs_AndSupportsClampAndWrap() {
            GameObject root = CreateGroup(out TabGroup group);
            try {
                TabButton first = CreateButton(root, "First");
                TabButton disabled = CreateButton(root, "Disabled");
                TabButton third = CreateButton(root, "Third");
                disabled.Toggle.interactable = false;
                group.Register(first);
                group.Register(disabled);
                group.Register(third);
                group.Select(first);

                Assert.That(group.NextTab(), Is.True);
                Assert.That(group.SelectedButton, Is.EqualTo(third));
                Assert.That(group.NextTab(), Is.False);
                Assert.That(group.SelectedButton, Is.EqualTo(third));

                group.Navigation = TabGroup.NavigationMode.Wrap;
                Assert.That(group.NextTab(), Is.True);
                Assert.That(group.SelectedButton, Is.EqualTo(first));
                Assert.That(group.PreviousTab(), Is.True);
                Assert.That(group.SelectedButton, Is.EqualTo(third));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void AllowNoSelection_TracksToggleSwitchOff() {
            GameObject root = CreateGroup(out TabGroup group);
            try {
                TabButton button = CreateButton(root, "Optional");
                group.Register(button);
                group.AllowNoSelection = true;
                group.Select(button);

                button.Toggle.isOn = false;

                Assert.That(group.SelectedIndex, Is.EqualTo(-1));
                Assert.That(group.SelectedButton, Is.Null);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RegisterAndUnregister_KeepSelectionAndPagesConsistent() {
            GameObject root = CreateGroup(out TabGroup group);
            try {
                TabButton first = CreateButton(root, "First");
                TabButton second = CreateButton(root, "Second");
                GameObject firstPage = new("FirstPage");
                GameObject secondPage = new("SecondPage");
                firstPage.transform.SetParent(root.transform, false);
                secondPage.transform.SetParent(root.transform, false);
                group.Register(first, firstPage);
                group.Register(second, secondPage);
                group.Select(first);

                Assert.That(group.Unregister(first), Is.True);

                Assert.That(group.Tabs.Count, Is.EqualTo(1));
                Assert.That(group.SelectedButton, Is.EqualTo(second));
                Assert.That(firstPage.activeSelf, Is.False);
                Assert.That(secondPage.activeSelf, Is.True);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Reducer_SelectsAndReportsEffectsWithoutUnityObjects() {
            TabSnapshot snapshot = new(
                new[] { true, true },
                allowNoSelection: false,
                wrapNavigation: false);

            TabTransition transition = TabStateReducer.Reduce(
                new TabState(-1),
                snapshot,
                new TabCommand(TabCommandType.Select, 1));

            Assert.That(transition.Accepted, Is.True);
            Assert.That(transition.State.SelectedIndex, Is.EqualTo(1));
            Assert.That(transition.Effects[0].Type, Is.EqualTo(TabEffectType.ApplySelection));
            Assert.That(transition.Effects[0].Index, Is.EqualTo(1));
            Assert.That(transition.Effects[1].Type, Is.EqualTo(TabEffectType.NotifySelected));
            Assert.That(transition.Effects[2].Type, Is.EqualTo(TabEffectType.NotifyIndexChanged));
        }

        [Test]
        public void Reducer_NavigationSkipsUnavailableTabsAndRejectsInvalidCommands() {
            TabSnapshot snapshot = new(
                new[] { true, false, true },
                allowNoSelection: false,
                wrapNavigation: false);

            TabTransition next = TabStateReducer.Reduce(
                new TabState(0),
                snapshot,
                new TabCommand(TabCommandType.Next));
            TabTransition outside = TabStateReducer.Reduce(
                next.State,
                snapshot,
                new TabCommand(TabCommandType.Next));

            Assert.That(next.Accepted, Is.True);
            Assert.That(next.State.SelectedIndex, Is.EqualTo(2));
            Assert.That(outside.Accepted, Is.False);
            Assert.That(outside.State.SelectedIndex, Is.EqualTo(2));
        }

        private static GameObject CreateGroup(out TabGroup group) {
            GameObject root = new("TabGroup", typeof(RectTransform), typeof(ToggleGroup), typeof(TabGroup));
            group = root.GetComponent<TabGroup>();
            group.AllowNoSelection = true;
            return root;
        }

        private static TabButton CreateButton(GameObject parent, string name, bool addImage = true) {
            GameObject buttonObject = addImage
                ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle), typeof(TabButton))
                : new GameObject(name, typeof(RectTransform), typeof(Toggle), typeof(TabButton));
            buttonObject.transform.SetParent(parent.transform, false);
            TabButton button = buttonObject.GetComponent<TabButton>();
            if(addImage)
                button.SetTargetGraphic(buttonObject.GetComponent<Image>());
            return button;
        }
    }
}
