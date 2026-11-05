# Coin Collector 3D — Pràctica UF2 (Nucli del joc)

Una bola que se mueve con física por un nivell, recoge monedas y se reinicia si cae a la KillZone.

- **Unity:** 6 LTS (6000.3.x)
- **Controles:** WASD / flechas

## Uso de IA

En esta práctica he usado IA (Claude, de Anthropic) como apoyo para:
- Escribir los scripts del juego (`PlayerController`, `Coin`, `KillZone`, `GameManager`, `MainMenu`, `Spinner`).
- Montar el nivel, el prefab de la moneda y el menú mediante un script de editor (`Assets/Editor/CoinCollectorBuilder.cs`).
- Redactar esta documentación y configurar Git (`.gitignore`, Git LFS).

Yo he creado el proyecto en Unity, he probado el juego y he revisado el resultado.

## Capturas

| Menú principal | Nivel 1 |
|---|---|
| ![Menú principal](Screenshots/menu.png) | ![Nivel 1 con las 8 monedas](Screenshots/nivel.png) |

**Recogiendo monedas** (la Console muestra la puntuación):

![Recogiendo monedas](Screenshots/monedas.png)

## Estructura

```
Assets/
├── Scenes/      Level1.unity, MenuPrincipal.unity
├── Scripts/     PlayerController, Coin, KillZone, GameManager, MainMenu, Spinner
├── Prefabs/     Coin.prefab
└── Materials/   Gold, Ground, Player
```

---

# Guía de montaje paso a paso

## 0. Crear el proyecto con Git

1. Unity Hub → **New project** → plantilla **Universal 3D** (o 3D Core) → nombre `CoinCollector3D`.
2. Marca **Connect to Unity Cloud / Source control provider** si lo ofrece, o créalo normal.
3. Copia en la raíz del proyecto (junto a `Assets/`) los archivos `.gitignore`, `.gitattributes` y este `README.md`.
4. Copia la carpeta `Assets/Scripts/` dentro de `Assets/` del proyecto.
5. Crea las carpetas `Assets/Scenes`, `Assets/Prefabs`, `Assets/Materials`.

En una terminal, dentro de la carpeta del proyecto:

```bash
git lfs install
git init
git add .
git commit -m "Proyecto inicial Coin Collector 3D"
```

Crea un repo vacío en GitHub y:

```bash
git remote add origin https://github.com/TU_USUARIO/CoinCollector3D.git
git branch -M main
git push -u origin main
```

> Si al darle a Play sale el error *"You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System package"*:
> **Edit → Project Settings → Player → Other Settings → Active Input Handling = Both** (Unity se reinicia).

## 1. Tags

**Edit → Project Settings → Tags and Layers** → añade el tag `Ground` (`Player` ya existe).

## 2. Materiales (`Assets/Materials`)

Click derecho → Create → Material:
- `Gold` — Base Map amarillo/dorado, Metallic ~0.8, Smoothness ~0.7
- `Ground` — gris o verde
- `Player` — azul o rojo

## 3. Escena Level1

File → New Scene → guárdala como `Assets/Scenes/Level1.unity`.

### Terra
- GameObject → 3D Object → **Plane**, nombre `Ground`
- Position (0,0,0), Scale (2,1,2) → mide 20×20
- Tag = `Ground`, material `Ground`

### Paredes (4)
GameObject → 3D Object → **Cube** ×4 (agrúpalas en un vacío `Walls`):

| Nombre | Position | Scale |
|---|---|---|
| Wall_N | (0, 1, 10)  | (20, 2, 0.5) |
| Wall_S | (0, 1, -10) | (20, 2, 0.5) |
| Wall_E | (10, 1, 0)  | (0.5, 2, 20) |
| Wall_W | (-10, 1, 0) | (0.5, 2, 20) |

En cada una: **desmarca Mesh Renderer** y marca **Box Collider → Is Trigger** (lo pide el enunciado).

> ⚠️ Un collider con *Is Trigger* **no bloquea** físicamente. Con las paredes en trigger la bola las atraviesa, cae del plano y la KillZone reinicia el nivel. Si quieres que la bola rebote contra ellas, deja *Is Trigger* desmarcado. Coméntalo en la defensa (o pregunta al profe cuál prefiere).

### KillZone
- Cube, nombre `KillZone`, Position (0, -10, 0), Scale (200, 1, 200)
- Desmarca **Mesh Renderer**, **Box Collider → Is Trigger** ✔
- Add Component → `KillZone`

### Player
- GameObject → 3D Object → **Sphere**, nombre `Player`, Position (0, 0.5, 0)
- Tag = `Player`, material `Player`
- Add Component → **Rigidbody**
  - Constraints → **Freeze Rotation X** ✔ y **Z** ✔ (el script también lo fuerza)
  - Interpolate = Interpolate (movimiento más suave)
- Add Component → `PlayerController` → Speed = 5

### GameManager
- GameObject → Create Empty, nombre `GameManager`
- Add Component → `GameManager`

### Cámara
Main Camera → Position (0, 18, -14), Rotation (50, 0, 0) para ver todo el nivel.

## 4. Prefab Coin

1. GameObject → 3D Object → **Sphere** (o Cylinder con Scale (0.6, 0.05, 0.6) y Rotation X=90 para que parezca moneda), nombre `Coin`.
2. Scale (0.5, 0.5, 0.5), Position Y = 0.5, material `Gold`.
3. **Sphere Collider → Is Trigger** ✔ (si usaste Cylinder, quita el Capsule Collider y añade un Sphere Collider).
4. Add Component → `Coin`.
5. (Ampliación) Add Component → `Spinner`.
6. Arrástralo a `Assets/Prefabs/` → se crea el prefab. Borra el de la escena.
7. Arrastra **al menos 6** monedas desde el prefab a la escena, repartidas por el nivel.

Guarda la escena (Ctrl+S).

## 5. Escena MenuPrincipal

1. File → New Scene → guárdala como `Assets/Scenes/MenuPrincipal.unity`.
2. GameObject → UI → **Canvas** (crea también el EventSystem).
3. Dentro del Canvas: GameObject → UI → **Button - TextMeshPro** (importa TMP Essentials si lo pide). Texto: `Play`.
4. Create Empty `MenuManager` → Add Component → `MainMenu`.
5. Botón → **On Click ()** → `+` → arrastra `MenuManager` → función `MainMenu.PlayGame`.

## 6. Build Settings

**File → Build Profiles** (en Unity 6) → **Scene List** → *Add Open Scenes* o arrastra:
- 0: `Scenes/MenuPrincipal`
- 1: `Scenes/Level1`

## 7. Probar

Abre `Level1` → Play:
- WASD mueve la bola sin que ruede.
- Al tocar moneda: desaparece y en la Console sale `Coin collected! Score: X`.
- Al caer por el borde: `Player killed!` y la escena se reinicia (score vuelve a 0).

Abre `MenuPrincipal` → Play → botón Play carga Level1.

## 8. Subir

```bash
git add .
git commit -m "Nucli del joc: nivell, player, monedes, GameManager, KillZone"
git push
```

Comprueba en GitHub que están `Assets/`, `Packages/` y `ProjectSettings/` (y **no** `Library/` ni `Temp/`).

---

## Chuleta para la defensa oral

- **FixedUpdate vs Update:** el input se lee en `Update` (cada frame); la física (`linearVelocity`) se aplica en `FixedUpdate` (paso fijo de física).
- **Conservar Y:** copiamos `rb.linearVelocity.y` para no anular la gravedad.
- **Normalize:** en diagonal el vector mide √2; normalizándolo la velocidad es igual en todas direcciones.
- **Freeze Rotation X/Z:** evita que la esfera ruede; se desliza.
- **Trigger vs Collision:** `Is Trigger` no choca, solo avisa con `OnTriggerEnter`. Las monedas y la KillZone son triggers.
- **CompareTag("Player"):** solo reaccionamos al jugador (y es más eficiente que `tag ==`).
- **Singleton:** `GameManager.Instance` es estático; en `Awake` si ya existe otro se destruye. Así cualquier script accede con `GameManager.Instance.AddScore(1)`.
- **RestartLevel:** `SceneManager.LoadScene(GetActiveScene().buildIndex)` recarga la escena actual → por eso la escena tiene que estar en Build Settings.
- **[SerializeField] private:** se ve en el Inspector sin hacer el campo público (encapsulación).
- **Ampliaciones:** `Spinner` gira con `transform.Rotate` × `Time.deltaTime` y flota con `Mathf.PingPong(Time.time * speed, altura)`.
