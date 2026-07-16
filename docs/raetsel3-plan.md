# Umsetzungsplan – Rätsel 3/5: Bild & Buch

Mini Escape-Room (AR SS 2026) · Mock 3 · UC-03
Belohnung: **Ziffer 2** (Teil des Gesamtcodes `5792`)

> Status des Projekts: Unity **6000.3.16f1**, URP + Input System vorhanden.
> **AR Foundation / ARCore / ARKit sind noch NICHT installiert** und es gibt noch
> keinen Spielcode. Dieser Plan beschreibt die Umsetzung — es wird hier bewusst
> noch kein Code geschrieben.

---

## 1. Ziel & Spielablauf (aus dem Konzept)

Der Spieler hat aus Rätsel 2 bereits die **Ziffer 5**. In Rätsel 3:

1. Ein **schief hängendes Bild** an der Wand wird per Touch **geradegerückt**.
2. Dadurch wird ein Hinweis sichtbar / ein **Buch** in der Umgebung wird auffindbar.
3. Der Spieler **sucht das Buch**, **öffnet** es und entdeckt die **Ziffer 2**.
4. Die Ziffer 2 wird an das zentrale Code-System übergeben.

Vorgeschlagener detaillierter Flow (Zustandsmaschine):

```
[Start R3]
   │  (Voraussetzung: Ziffer 5 aus R2 vorhanden)
   ▼
[Bild schief] ──Touch-Rotation──► [Bild fast gerade] ──Toleranz erreicht──► [Bild rastet ein]
   │
   ▼ (Snap löst Event aus: Buch-Hinweis/Highlight aktivieren)
[Buch suchen] ──Spieler findet & tippt Buch──► [Buch öffnen-Animation]
   │
   ▼
[Ziffer 2 anzeigen] ──► [Ziffer 2 an CodeManager] ──► [R3 gelöst]
```

---

## 2. Voraussetzungen / Abhängigkeiten

- **AR-Grundgerüst** (separates Issue/AR-Setup): Plane Detection + stabile
  Verankerung von Objekten. Falls noch nicht vorhanden, kann Rätsel 3 zunächst in
  einer **Test-Szene ohne AR** entwickelt und später an einen AR-Anchor gehängt werden.
- **Gemeinsames Code-System** (`CodeManager`) mit einer Schnittstelle zur
  Ziffern-Übergabe (siehe Abschnitt 6). Sollte vor allen Rätseln definiert werden.
- **Reihenfolge:** R3 wird erst aktiv, wenn R2 (Ziffer 5) gelöst ist — über den
  zentralen Spielzustand/`GameManager` steuerbar.

---

## 3. Benötigte Assets

| Asset | Beschreibung | Label |
|-------|--------------|-------|
| Bild (gerahmt) | Gerahmtes Wandbild, anfangs schief rotiert | `assets` |
| Wand (optional) | Virtuelle Wandfläche als Aufhängepunkt | `assets`/`ar` |
| Buch | Auf-/zuklappbares Buch mit Animation; Innenseite zeigt „2" | `assets` |
| Ziffer „2" | Als Textur/3D-Text/Sprite auf der Buchseite | `assets`/`ui` |
| Sounds | Einrast-Sound (Bild), Blätter-/Öffnen-Sound (Buch) | `audio` |
| Highlight-Material | Hervorhebung des Buchs nach Bild-Lösung | `assets` |

Quelle laut Konzept: Unity Asset Store (kostenlose Assets) bzw. ChatGPT-Prompts für
Mockups.

---

## 4. Szenen- / Prefab-Struktur

```
Raetsel3 (leeres GameObject / Prefab, gehängt an AR-Anchor)
├── Bild
│   ├── Rahmen (Mesh + Collider)
│   └── PictureStraighten (Skript)
├── Buch
│   ├── Buch_Mesh (Animator: Open/Close)
│   ├── Seite_Ziffer2 (anfangs inaktiv/verdeckt)
│   └── BookInteraction (Skript)
├── Highlights (anfangs inaktiv)
└── Raetsel3Controller (Skript: Zustandsmaschine für R3)
```

- **Empfehlung:** Rätsel 3 als **eigenes Prefab** bauen → leichteres Testen,
  saubere Platzierung über den AR-Platzierungsmechanismus.

---

## 5. Skript-Architektur (Verantwortlichkeiten, ohne Code)

> Vorgeschlagener Namespace: `EscapeRoom.Puzzles`. Ablage z. B. unter
> `Assets/Scripts/Puzzles/`.

- **`IPuzzle` (Interface, projektweit)**
  - `event Action<int> OnSolved` — liefert die gelöste Ziffer.
  - `void Activate()` / `bool IsSolved { get; }`.
  - Ermöglicht einheitliche Behandlung aller 5 Rätsel.

- **`Raetsel3Controller : MonoBehaviour, IPuzzle`**
  - Hält die Zustandsmaschine (Bild → Buch → gelöst).
  - Aktiviert das Rätsel erst, wenn Vorbedingung (R2 gelöst) erfüllt ist.
  - Abonniert `PictureStraighten.OnStraightened` und `BookInteraction.OnOpened`.
  - Feuert `OnSolved(2)` und meldet an den `CodeManager`.

- **`PictureStraighten : MonoBehaviour`**
  - Verarbeitet Touch-Drag → Rotation des Bildes um die Wand-Normale (eine Achse).
  - Prüft den Restwinkel gegen eine **Toleranz** (z. B. ±5°).
  - Bei Erfolg: Snap auf 0°, Sound/Feedback, `event OnStraightened`.

- **`BookInteraction : MonoBehaviour`**
  - Reagiert auf Touch/Raycast-Treffer auf das Buch.
  - Spielt Öffnen-Animation, blendet die Ziffer „2" ein.
  - `event OnOpened`.
  - Wird (optional) erst nach `OnStraightened` interaktiv/hervorgehoben.

- **Eingabe-Helfer (geteilt): `TouchRaycaster`**
  - Wandelt Touch-/Klick-Position via Kamera-Raycast in getroffene Objekte um
    (Input System, `Pointer.current`). Von mehreren Rätseln nutzbar.

---

## 6. Integration mit Code-System & GameManager

- **`CodeManager` (zentral)**
  - Speichert die Zuordnung *Rätsel → Ziffer* (z. B. `Dictionary<int,int>`).
  - Erwarteter Gesamtcode: **5792**.
  - Methode `SubmitDigit(int puzzleId, int digit)`; bei Vollständigkeit Event
    `OnCodeComplete` für Rätsel 5.
- **`GameManager`**
  - Steuert die Rätsel-Reihenfolge / Freischaltung (R3 nach R2).
  - Hält den Gesamt-Spielzustand (auch für späteres Speichern lokal).
- **Vorgehen für R3:** `Raetsel3Controller.OnSolved` → `CodeManager.SubmitDigit(3, 2)`.

> Wichtig: Das Interface `IPuzzle` + `CodeManager.SubmitDigit(...)` zuerst im Team
> festlegen, damit alle Rätsel parallel entwickelt werden können.

---

## 7. Interaktionsdetails

### 7.1 Bild geraderücken
- **Rotationsachse:** nur um die Wand-Normale (Z bzw. lokale Forward-Achse), damit
  das Bild flach an der Wand bleibt.
- **Eingabe:** Touch-Drag links/rechts → Winkeländerung; alternativ Zwei-Finger-Drehung.
- **Startzustand:** Zufalls-/fester Schiefwinkel (z. B. -20° bis -30°).
- **Toleranz:** `abs(currentAngle) <= 5°` gilt als „gerade".
- **Snap:** bei Erreichen sanft auf exakt 0° interpolieren (Lerp) + Einrast-Sound.
- **Feedback:** kurzes Aufleuchten / haptisches Feedback (Handheld.Vibrate optional).

### 7.2 Buch suchen & öffnen
- Buch zunächst unauffällig/teilweise verdeckt platzieren („suchen").
- Nach Bild-Lösung: **Highlight** aktivieren (Outline/Glow) als Hinweis.
- Touch auf Buch → Öffnen-Animation (Animator-Trigger) → Ziffer „2" einblenden.
- Nach Anzeige: Ziffer ans Code-System melden, Rätsel als gelöst markieren.

---

## 8. Akzeptanzkriterien (Definition of Done)

- [ ] Bild startet schief und ist per Touch um eine Achse drehbar.
- [ ] Innerhalb Toleranz rastet das Bild auf 0° ein, mit Sound/visuellem Feedback.
- [ ] Buch ist auffindbar und nach Bild-Lösung hervorgehoben.
- [ ] Touch öffnet das Buch (Animation), Ziffer **2** wird sichtbar.
- [ ] Ziffer 2 wird genau einmal an den `CodeManager` übergeben.
- [ ] R3 ist nur nach gelöstem R2 spielbar.
- [ ] In AR: Bild & Buch bleiben beim Kamerabewegen stabil verankert.
- [ ] Kein erneutes Auslösen nach Lösung (idempotent).

---

## 9. Testplan

1. **Editor-Test (ohne AR):** Test-Szene mit Maus = Touch; Bild drehen, Snap prüfen,
   Buch öffnen, Konsole zeigt „Ziffer 2 an CodeManager".
2. **Toleranz-Test:** Bild knapp außerhalb/innerhalb ±5° → korrektes Snap-Verhalten.
3. **Reihenfolge-Test:** R3 vor R2 → Rätsel inaktiv; nach R2 → aktiv.
4. **Idempotenz:** Buch mehrfach antippen → Ziffer nur einmal gewertet.
5. **AR-Gerätetest:** Auf Android (ARCore) platzieren, Stabilität beim Bewegen prüfen.

---

## 10. Aufwandsschätzung (grob)

| Teilaufgabe | Aufwand |
|-------------|---------|
| Assets beschaffen/einbinden | 0,5 Tag |
| `IPuzzle` + `CodeManager`-Schnittstelle (gemeinsam) | 0,5 Tag |
| `PictureStraighten` (Touch-Rotation + Snap) | 1 Tag |
| `BookInteraction` (Öffnen + Ziffer) | 0,5 Tag |
| `Raetsel3Controller` (Zustandsmaschine + Integration) | 0,5 Tag |
| Feedback (Audio/Highlight) + Polish | 0,5 Tag |
| Tests (Editor + Gerät) | 0,5 Tag |
| **Summe** | **~4 Tage** |

---

## 11. Offene Fragen / Entscheidungen

- Wird das Bild an einer **erkannten realen Wand** (vertikale Plane Detection) oder
  an einer **virtuellen Wand** über der horizontalen Ebene aufgehängt?
  (Vertikale Plane Detection ist aufwändiger; virtuelle Wand ist einfacher und
  passt zum MVP.)
- Soll das Buch **frei in der Umgebung** platziert sein (mehr „Suchen") oder fest
  am Rätsel-Prefab (einfacher, planbarer)?
- Genaues visuelles Design der Ziffer „2" (Textur vs. 3D-Text vs. UI-Overlay).
- Soll der Bild-Schiefwinkel **zufällig** oder **fest** sein (Reproduzierbarkeit für Tests).

---

## 12. Nächste konkrete Schritte

1. Team-Abstimmung: `IPuzzle` + `CodeManager.SubmitDigit(...)` festziehen.
2. Entscheidung zu den offenen Fragen aus Abschnitt 11.
3. Assets (Bild, Buch) auswählen.
4. Test-Szene `Raetsel3_Test` ohne AR anlegen.
5. Skripte gemäß Abschnitt 5 implementieren (separates Coding-Issue/PR).
6. In AR-Setup integrieren und auf Gerät testen.
