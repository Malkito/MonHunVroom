# Build flow — MonHunVroom (revisado 2026-11-06, `main` @ 5a90d120)

## Flujo real del juego

```
MainMenu  (Assets/Matt Testing/Scenes/MainMenu.unity)
  │  Botón "lobby" → panel de creación de lobby (dentro de la misma escena, no cambia de escena)
  │  StartGame()  → Loader.LoadNetwork(FantasyGameScene)      [MainMenuManager.cs:21]
  ▼
FantasyGameScene  (Assets/Scenes/GAME SCENES/FantasyGameScene.unity)
  │  Ronda ganada → portal; cuando todos los jugadores entran → TronGameScene   [roundWonManager.cs:71-78]
  │  Derrota → recarga FantasyGameScene                                         [GameStateManager.cs:124-134]
  ▼
TronGameScene  (Assets/Scenes/GAME SCENES/TronGameScene.unity)
  │  Ronda ganada → portal → FantasyGameScene
  │  Derrota → recarga TronGameScene
  └─► (bucle Fantasy ⇄ Tron)

Volver al menú: MainMenuManager.mainMenu() → SceneManager.LoadScene(0)  (índice 0 del Build)
```

- Los cambios de escena en juego son de red: `Loader.LoadNetwork(...)` usa `NetworkManager.SceneManager.LoadScene` (todos los clientes cambian juntos). El host tiene que haber arrancado la sesión en el MainMenu.
- MainMenu, FantasyGameScene, TronGameScene, LobbyScene y MonsterPlayerGameScene tienen cada una su propio `NetworkManager`.
- El monstruo (`Assets/Prefabs/Placeholder/MONSTER.prefab`) está en Fantasy y en Tron.
- Day/Tron/Fantasy por selección de nivel: el `switch` está comentado en `MainMenuManager.StartGame()`; ahora siempre va a Fantasy.

## Build Settings (ProjectSettings/EditorBuildSettings.asset)

| # | Escena | ¿Existe? | ¿Se usa en el flujo? |
|---|---|---|---|
| 0 | Multiplayer Testing/NGO_Minimal_Setup/NGO_Setup | **No** | — |
| 1 | Matt Testing/Scenes/MainMenu | Sí | **Sí (entrada)** |
| 2 | Matt Testing/Scenes/DayGameScene | Sí | No (switch comentado) |
| 3 | Matt Testing/Scenes/NightGameScene | Sí | No |
| 4 | Matt Testing/Scenes/LobbyScene | Sí | Antiguo: TestingLobbyUI → CharacterSelect |
| 5 | Scenes/NewNateTesting | **No** | — |
| 6 | Matt Testing/Scenes/MonsterPlayerGameScene | Sí | No |
| 7 | Matt Testing/Scenes/CharacterSelect | Sí | Antiguo: → `PVPScene` (no existe) |
| 8 | Widgets/Scenes/WidgetsQuickstart | Sí | No |
| 9 | Scenes/MonsterScene | **No** | — |
| 10 | Scenes/GAME SCENES/TronGameScene | Sí | **Sí** |
| 11 | Scenes/GAME SCENES/FantasyGameScene | Sí | **Sí** |

Unity quita del build las escenas que no existen, así que **MainMenu acaba siendo el índice 0** y `LoadScene(0)` funciona. Igual conviene que el equipo limpie las 3 entradas rotas (no es tarea mía: avisar).

## Mis escenas de prueba (esta carpeta)

- Aquí van mis copias: `Juan_FantasyGameScene.unity` (y si hace falta `Juan_TronGameScene.unity`).
- Hacer la copia **desde Unity** (seleccionar la escena en Project → Ctrl+D → mover aquí → renombrar). Nunca copiar el `.unity` a mano: duplicaría el GUID.
- En la copia, reemplazar `MONSTER` por mi variante con el Kaiju. No tocar las escenas originales ni `MONSTER.prefab`.
- Probar directo: abrir mi copia y darle Play. **Corregido 2026-11-07:** el `MultiplayerManager` (NetworkManager) viene **desactivado** en las escenas del equipo (también en la original) porque la sesión se arranca desde el MainMenu; sin él no hay red, `readyCheck.Start` da NullReference y no aparece el jugador. En mi copia, el objeto `JuanAutoHost` (script `JuanAutoHost.cs`, solo en Editor) lo activa y arranca el Host al dar Play.
- La escena tampoco tiene NavMesh (`NavMeshSurface.m_NavMeshData` apunta a un asset que no existe), así que el monstruo no camina hasta hornearlo.
- Mis copias **no se añaden a Build Settings** (cambiaría el flujo del equipo).
- **Decisión (2026-11-06): mis copias SÍ se suben al repo.**
- Peso: la copia de Fantasy son ~95 MB y el repo no usa Git LFS. GitHub rechaza archivos de más de 100 MB, así que si la copia pasa de 100 MB el push falla. Antes de cada push, revisar el tamaño del `.unity`; si se acerca, usar la copia de Tron (~23 MB) o una escena más pequeña.

## Ojo para probar el flujo completo con mi monstruo

Los portales cargan las escenas **por nombre** (`Loader.Scene`), así que desde mi copia el portal me lleva a la escena original del equipo, no a la mía. Para probar ataques y animaciones basta con mi copia; el flujo completo se prueba cuando el Kaiju esté en el prefab real.
