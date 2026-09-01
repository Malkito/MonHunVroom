# Niki.UI — HUD wiring guide (for designers)

One sentence: **each visual element is bound to a "value" (a property) on a small
script called a *view model*.** Game code writes the values; the UI updates itself.
You never wire up per-frame updates and you never poll anything in `Update()`.
Your job is to **place, reference, and tune the look**. A programmer's job is to
**feed the values**.

---

## 1. The 30-second version

```
game state (upgrade data, cooldown timers, inputs, health…)
        │  programmer writes values
        ▼
View Model  (AbilitySlotViewModel etc. — one per HUD widget)
        │  fires "value changed" events
        ▼
Bindable elements  (small components on the Image / Slider / Text objects)
        │  apply the value to the UI component
        ▼
What you see (sprite, color, slider fill, text, cooldown wedge)
```

The binding is automatic and one-way. If the value doesn't actually change, the UI
skips the work. If the GameObject is destroyed, the binding cleans itself up.

---

## 2. Creating an ability slot

1. Make sure the **Canvas** you want the slot to live under is in the scene
   (or open the prefab that contains it).
2. Menu: **`Niki UI → Create Ability Slot (in active scene)`**.
3. A fully wired `AbilitySlot` (140 × 140, center-anchored) appears under the first
   Canvas, with everything already referenced. Select it and move/scale/anchor it
   like any other RectTransform.
4. For abilities 2 and 3: **duplicate the `AbilitySlot` GameObject** (Ctrl+D).
   Each copy gets its own view model — no extra wiring needed.

> The menu works while editing a scene **or** while a prefab is open, so you can
> build the slots directly inside the PlayerUI prefab.

---

## 3. What you're looking at

```
AbilitySlot          [AbilitySlotWidget, BindableImageSprite, BindableBoolColor]
├── Icon             [Image]
├── RadialCooldown   [Image, BindableRadialCooldown]
└── Name             [TextMeshProUGUI, BindableText]
```

### Inspector cheat sheet — what to touch, what's already wired

| Where | What it controls | Default |
|---|---|---|
| `Icon → Image → Sprite` | The icon picture. **Preview only** — at runtime the upgrade data overwrites it with `UpgradeScriptableOBJ.IconImage`. | white square |
| `RadialCooldown → Image → Color` | **The dim color of the cooldown wedge.** This is your main visual knob. | black @ 55% alpha |
| `RadialCooldown → Bindable Radial Cooldown → Radial Material` | The shader material. **Don't touch** — it's the `NikiUI/RadialCooldown` material and is auto-created. | auto |
| Root `→ Bindable Bool Color → True Color / False Color` | The icon tint **while the ability input is held** (press feedback) vs. released. | white / white @ 50% |
| `Name → TextMeshPro` | Font, size, color, alignment of the label. | 14 pt, centered, "?" placeholder |

Everything else (the references between the `Bindable…` components and the widget)
is wired by the menu — you never need to drag anything in the Inspector.

### The radial cooldown, visually

- The value is `1.0` = **just used** (a full dim circle over the icon), `0.0` =
  **ready** (nothing drawn, icon fully visible).
- The wedge **starts at 12 o'clock and sweeps clockwise**: right after a use the
  whole circle is dim, and as the countdown runs the dim area shrinks back toward
  12 o'clock until the slot is ready.
- The wedge is clipped to a circle, so even the default white square sprite reads
  as a circular overlay on top of your icon.

---

## 4. Per-player HUD (multiplayer — important)

- Every player's `PlayerUI` must **start deactivated** in the prefab, otherwise
  all players' HUDs overlap when they join.
- Enable the HUD **only for the local player** (the existing `createPlayerUI`
  pattern already does this — keep it).
- If you add HUD pieces to a player prefab, **assign them on both tanks**
  (`TronTank` and `FantasyTank`).
- When a player leaves/is destroyed, the whole `PlayerUI` GameObject can just be
  `Destroy`ed — all bindings release automatically. No manual cleanup.

---

## 5. What the programmer needs to do (hand-off list)

One short snippet per slot, run each frame (or on relevant model events) for the
local player:

```csharp
var vm = slotWidget.ViewModel;              // the slot's view model
var def = UpgradeDatabase.Instance.Get(slotId);

vm.Icon.Value            = def.IconImage;              // sprite
vm.Name.Value            = def.name;                  // label text
vm.CooldownRemaining.Value = Mathf.Clamp01(remainingSeconds / def.cooldown); // 1→0
vm.IsPressed.Value       = GameInput.instance.getAbilityOneInput();           // press feedback
```

That's all the UI side needs. The radial wedge, icon sprite, name and press tint
all react to those writes automatically.

Other HUD pieces, same pattern:

- **Health / mana / boost bar:** one `BindableSliderFill` on a `Slider` + one
  `Property<float>` in a small view model. No new binding code needed.
- **Buttons:** a Button's click handler calls `viewModel.Activate.Execute()` (or
  the model layer sets the action via `Activate.SetAction(...)`).

---

## 6. Under the hood (for programmers / later)

Lean re-implementation of the UnityMvvmToolkit pattern
(`github.com/LibraStack/UnityMvvmToolkit`): view model + observable properties +
per-element bindable widgets, written from scratch for this project's uGUI stack.

```
Assets/Niki/UI/
├── Runtime/
│   ├── Niki.UI.Runtime.asmdef
│   ├── Core/
│   │   ├── IBindingContext.cs     view-model marker
│   │   ├── IProperty.cs           IProperty<T> / IReadOnlyProperty<T>
│   │   ├── Property.cs           Property<T> (event on real change only), ReadOnlyProperty<T>
│   │   ├── Command.cs            ICommand / Command (view → model call)
│   │   └── BindableElement.cs    widget base: binds 1 property → 1 UI element, auto-unbind on destroy
│   ├── Elements/
│   │   ├── BindableImageSprite.cs   Sprite  → Image.sprite
│   │   ├── BindableImageColor.cs    Color   → Image.color
│   │   ├── BindableBoolColor.cs     bool    → Image.color (true/false tints)
│   │   ├── BindableSliderFill.cs    float   → Slider.value
│   │   ├── BindableText.cs          string  → TMP_Text.text
│   │   └── BindableRadialCooldown.cs float  → radial wedge (shader _Fill, 1 = full, 0 = clear)
│   ├── Shaders/
│   │   └── RadialCooldown.shader    "NikiUI/RadialCooldown" (URP-compatible)
│   └── Widgets/
│       ├── AbilitySlotViewModel.cs  Icon, Name, CooldownRemaining, IsPressed, Activate
│       └── AbilitySlotWidget.cs     wires the elements to the view model
└── Editor/
    └── NikiUiBuilder.cs             menu "Niki UI > Create Ability Slot (in active scene)"
```

Notes:

- `Property<T>` only fires `ValueChanged` when the value actually changes, so
  per-frame writes are cheap.
- `AbilitySlotViewModel` is a plain C# class (not serializable), so the widget
  recreates it in `Awake` and re-binds the serialized element references — the
  slot keeps working across play sessions and scene reloads.
- Extending = add a `*ViewModel` + `*Widget` pair, or a new `BindableElement<T>`
  subclass for a new rendering capability. Everything else (bindings, lifecycle,
  teardown) comes from the base class.

---

## 7. Full UI hook inventory (existing placeholder scripts)

Reference list of every UI hook the HUD needs, per system — these are the
variables the new widgets/views bind against instead of the placeholder scripts.
Every system below is annotated with the Niki.UI element(s) that drive it.
All of it is covered by existing elements **except** the two small new elements
listed under **Elements to add** — the only gaps.

### 1. Power Up / Ability HUD (new script — don't build on feedbackUI)

Per ability slot (×3), on each player's PlayerUI:

- `Image abilityIcon` (Sprite) ← set from `UpgradeScriptableOBJ.IconImage`
- `TMP_Text abilityName` ← `UpgradeScriptableOBJ.itemDesc` / name
- Cooldown visual (Image fill + color) ← `playerUpgradeManager.abilityOne/Two/ThreeCooldown` and `EquippedUpgrade.cooldownRemaining` + `canUseUpgrade`
- Input-press feedback color ← `GameInput.instance.getAbilityOneInput/Two/Three`
- Data sources: `playerUpgradeManager.equipped[3]` (`EquippedUpgrade`: `upgradeID`, `logicInstance`, `logicScript`, `cooldownRemaining`), looked up via `UpgradeDatabase.Instance`
- Relevant fields on `UpgradeScriptableOBJ`: `upgradeID`, `IconImage`, `itemDesc`, `cooldown`, `isAvailble`, `canBeUsedWhileDead`
- **Wired with (no new code):** `AbilitySlotWidget` — `BindableImageSprite` (icon), `BindableText` (name), `BindableRadialCooldown` (cooldown, 1→0), `BindableBoolColor` (input-press feedback + `canUseUpgrade` dim)

### 2. Player Health bar (playerHealth.cs)

- `Slider healthSlider` ← `currentHealth.Value / baseMaxHealth`
- `float baseMaxHealth` (slider max)
- `NetworkVariable<float> currentHealth`
- `GameObject deathUI` (overlay when health hits 0)
- Damage-flash: `Color damageColour`, `Color fireColour`, `float flashTIme`
- (Check `playerStats` if max health can be modified by upgrades)
- **Wired with:** `BindableSliderFill` (bar), `BindableImageColor` (damage flash — the model computes the flash color over `flashTIme`, the UI just displays it), `BindableSetActive` (`deathUI` overlay)

### 3. Fantasy Mana bar (ManaSystem.cs)

- `Slider ManaSLider` ← `currentMana`, `maxValue = maxMana`
- `float maxMana`, `float manaUsedPerActivation`
- Regen: `float ManaRegenAmount`, `float timeBeforeRegen`
- Boost scaling: `playerStats.currentSpecialBoost` (affects regen rate)
- **Wired with (no new code):** `BindableSliderFill`

### 4. Tron Speed boost (speedBoost.cs)

- `Slider boostUi` ← `currentboost`, `maxValue = MaxBoost`
- `TMP_Text speedBoostText` ← rounded `currentboost`
- `float MaxBoost` (100 + `currentSpecialBoost * 20`), `currentboost`, `boostGainedOnPickup`
- (VFX, not UI, but related: `TrailRenderer[] Trials`, `ParticleSystem windParticles`, Cinemachine FOV)
- **Wired with (no new code):** `BindableSliderFill` (bar) + `BindableText` (the model formats the rounded number as a string)

### 5. Jump icon (NewTankMovement.cs)

- `Image jumpIcon` ← color/state driven by `jumpTimer` vs `MaxJumpTimer` (cooldown indicator)
- **Wired with (no new code):** `BindableRadialCooldown` fed by `jumpTimer / MaxJumpTimer` (or `BindableBoolColor` for a simple dim)

### 6. "Main Building" health bar

- ⚠️ Heads-up: no `ProtectTheBuiling` script exists in the project. The closest is
  `BuildingHealth.cs` (`Assets/Matt Testing/Scripts/World/`), which has `public float maxHealth`
  and `public float currentHealth` but **no UI slider hookup yet** — you'll likely need to
  add a `Slider`/`Image` reference there or on an objective-HUD script.
  Worth confirming with him which object/scene this lives on.
- **Wired with (no new code):** `BindableSliderFill` fed from `BuildingHealth.maxHealth / currentHealth` — the missing piece is the game-code hookup in `BuildingHealth.cs`, not UI code.

### 7. Ready check canvas + tutorial (readyCheck.cs, on readyCheckManager)

- `GameObject readyCanvas`
- `Image[] readyCheckImages` — one per player slot, activated per connected client
- `Button readyButton` — interactable state; also ready via `GameInput.instance.getJumpInput()`
- Player counts: `numOfPlayers` / `numOfPlayersReady` (candidate for a "X/Y ready" label)
- Tutorial: `TMP_Text` content on the canvas (text to swap out with final copy)
- **Wired with:** `BindableText` ("X/Y ready" label + tutorial copy), `BindableBoolColor` per ready-slot image (active per connected client), `Command` (the ready action) — plus the two new elements: `BindableSetActive` (`readyCanvas`) and `BindableButtonEnabled` (`readyButton`)

### 8. Power up choice (powerUpSpawnPoolManager.cs)

- `Image[] IconSprites` (3 slots) ← `UpgradeScriptableOBJ.IconImage`
- `TMP_Text[] upgradeNames` (3 slots) ← upgrade name
- 3 Buttons ← wired to `FirstUpgrade()` / `SecondUpgrade()` / `ThirdUpgrade()`
- `GameObject upgradeChoiceUI` — the panel toggled on/off (`SetActive`)
- `int amountOfUpgradesToBeAvailble` (= 3), `UpgradeScriptableOBJ[] availbleUpgrades` / `spawnPool` / `entireUpgradePool`
- **Wired with:** `BindableImageSprite` ×3, `BindableText` ×3, Buttons → `Command` (`ChooseOne/Two/Three`) — plus the two new elements: `BindableSetActive` (`upgradeChoiceUI` panel) and `BindableButtonEnabled` (the 3 buttons)

### Elements to add (the only gaps — ~15 lines each, same pattern as the existing ones)

- `BindableSetActive` — `bool → GameObject.SetActive` (used by `deathUI`, `readyCanvas`, `upgradeChoiceUI`)
- `BindableButtonEnabled` — `bool → Button.interactable` (+ optional grey tint on the button's image; used by `readyButton` and the 3 choice buttons)

### Cross-cutting

- `GameInput.instance` — all input-press feedback (abilities, sprint, jump/ready)
- `playerStats` NetworkVariables (`currentSpecialBoost`, `currentCooldownReduction`) —
  stats that shift UI values
- `createPlayerUI.cs` — `GameObject HUD` (the PlayerUI) enables only for
  `IsLocalPlayer`; it must start deactivated so multiple players' UIs don't stack up
- **Niki.UI coverage of the above:** per-frame input reads are written into `bool` properties each frame (`Property<T>` suppresses no-op writes); `playerStats` NetworkVariables feed the view models via `NetworkVariable.OnValueChanged`; the PlayerUI lifecycle (`createPlayerUI`) is plain GameObject lifecycle and composes with the framework as-is.
