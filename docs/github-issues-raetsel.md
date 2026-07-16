# GitHub Issues – Rätsel 1–5 (Mini Escape-Room)

Fertige Issue-Vorlagen zum manuellen Anlegen unter
`https://github.com/Hylarie-Nzeye/AR_Projekt/issues`.

So nutzt du diese Datei:
- Für jedes Rätsel ein eigenes Issue anlegen (Titel = Überschrift).
- Den Block **Beschreibung / Akzeptanzkriterien / Aufgaben** in das Issue-Textfeld kopieren.
- Die unter **Labels** genannten Labels setzen (siehe Abschnitt „Empfohlene Labels").
- Optional alle 5 Issues einem Meilenstein **„MVP Level 1"** zuordnen.

---

## Empfohlene Labels (einmalig anlegen)

| Label | Farbe (Hex) | Zweck |
|-------|-------------|-------|
| `puzzle` | `#5319e7` | Kennzeichnet ein Rätsel |
| `gameplay` | `#0e8a16` | Spiellogik / Interaktion |
| `ar` | `#1d76db` | AR-/Plane-/Platzierungs-Bezug |
| `ui` | `#fbca04` | UI / HUD / Hinweise |
| `audio` | `#c5def5` | Sound / Stimme |
| `assets` | `#d4c5f9` | 3D-Modelle, Texturen, Sounds |
| `MVP` | `#b60205` | Verbindlicher MVP-Umfang |

Empfohlener Meilenstein: **MVP Level 1**

Zahlen-Übersicht (Gesamtcode an der Tür: **5792**):

| Rätsel | Belohnung (Ziffer) |
|--------|--------------------|
| Rätsel 1 – Radio & Spiegel | `7` |
| Rätsel 2 – Schrank & Tresor | `5` |
| Rätsel 3 – Bild & Buch | `2` |
| Rätsel 4 – Katze füttern | `9` |
| Rätsel 5 – Code & Abschluss | – (Eingabe `5792`) |

> Hinweis: Die Reihenfolge der gesammelten Ziffern (7, 5, 2, 9) entspricht nicht
> der Eingabereihenfolge des Codes (5-7-9-2). Das ist gewollt und Teil des Rätsels.

---

## Issue 1 — Rätsel 1/5: Radio & Spiegel

**Labels:** `puzzle`, `gameplay`, `ar`, `MVP`
**Meilenstein:** MVP Level 1
**Bezug:** UC-03 (Objekt interagieren), Mock 1

### Beschreibung
Erstes Rätsel nach erfolgreicher Flächenerkennung. Der Spieler interagiert über
den Touchscreen mit einem Radio und einem Spiegel, um die erste Code-Ziffer zu erhalten.

### Akzeptanzkriterien
- [ ] Radio- und Spiegel-Objekt werden stabil in der AR-Ebene verankert.
- [ ] Touch-Interaktion mit Radio (z. B. Drehknopf / Sender einstellen) ist möglich.
- [ ] Touch-Interaktion mit Spiegel (z. B. ausrichten) ist möglich.
- [ ] Korrekte Lösung liefert das Code-Fragment **Ziffer 7** an das Code-System.
- [ ] Visuelles/akustisches Feedback bei Lösung.

### Aufgaben
- [ ] 3D-Assets Radio + Spiegel beschaffen/einbinden (`assets`)
- [ ] Interaktions-Skripte (Touch) implementieren
- [ ] Lösungslogik + Übergabe der Ziffer an `CodeManager`
- [ ] Feedback (Sound/Animation)

---

## Issue 2 — Rätsel 2/5: Schrank & Tresor

**Labels:** `puzzle`, `gameplay`, `ar`, `audio`, `MVP`
**Meilenstein:** MVP Level 1
**Bezug:** UC-03, Mock 2

### Beschreibung
Der Spieler hört eine Stimme, erkundet einen Schrank mit 3 Schubladen, liest ein
Papier, öffnet einen Tresor mit dem darauf basierenden Code, erhält einen Schlüssel,
befreit eine Taube und erhält als Belohnung die **Ziffer 5**.

### Akzeptanzkriterien
- [ ] Schrank mit 3 interaktiven Schubladen ist platziert und bedienbar.
- [ ] Papier kann gefunden und gelesen werden (zeigt Tresor-Hinweis).
- [ ] Tresor lässt sich nur mit korrektem Code öffnen.
- [ ] Schlüssel wird nach Tresoröffnung erhalten und befreit die Taube.
- [ ] Stimme/Audio wird zum richtigen Zeitpunkt abgespielt.
- [ ] Lösung liefert das Code-Fragment **Ziffer 5**.

### Aufgaben
- [ ] 3D-Assets Schrank, Schubladen, Tresor, Papier, Schlüssel, Taube (`assets`)
- [ ] Schubladen-/Tresor-Interaktion implementieren
- [ ] Audio-Trigger (`audio`)
- [ ] Lösungslogik + Übergabe der Ziffer an `CodeManager`

---

## Issue 3 — Rätsel 3/5: Bild & Buch

**Labels:** `puzzle`, `gameplay`, `ar`, `ui`, `MVP`
**Meilenstein:** MVP Level 1
**Bezug:** UC-03, Mock 3

### Beschreibung
Der Spieler besitzt bereits die Ziffer 5. In Rätsel 3 muss er ein schief hängendes
**Bild an der Wand geraderücken** und ein **Buch in der Umgebung suchen**, dieses
öffnen und so die **Ziffer 2** entdecken.

> Detaillierter technischer Umsetzungsplan: siehe `docs/raetsel3-plan.md`.

### Akzeptanzkriterien
- [ ] Ein schief platziertes Bild ist an einer (virtuellen) Wand verankert.
- [ ] Der Spieler kann das Bild per Touch drehen, bis es „gerade" ist.
- [ ] Bei Erreichen der geraden Position rastet das Bild ein (Snap) + Feedback.
- [ ] Ein Buch ist in der Szene auffindbar und per Touch zu öffnen.
- [ ] Beim Öffnen des Buchs wird die **Ziffer 2** sichtbar und an das Code-System übergeben.
- [ ] Reihenfolge/Abhängigkeit zu Rätsel 2 ist berücksichtigt (Ziffer 5 vorhanden).

### Aufgaben
- [ ] 3D-Assets Bild (gerahmt) + Buch (auf-/zuklappbar) (`assets`)
- [ ] Geraderücken-Mechanik (Touch-Rotation + Winkel-Toleranz + Snap)
- [ ] Buch-Such-/Öffnen-Mechanik
- [ ] Anzeige der Ziffer 2 (UI/3D) + Übergabe an `CodeManager`
- [ ] Feedback (Sound/Highlight)

---

## Issue 4 — Rätsel 4/5: Katze füttern

**Labels:** `puzzle`, `gameplay`, `ar`, `MVP`
**Meilenstein:** MVP Level 1
**Bezug:** UC-03, Mock 4

### Beschreibung
Der Spieler findet eine weinende Katze, sammelt drei Zutaten (Thunfisch, Reis, Milch),
kocht daraus eine Mahlzeit und erhält als Belohnung die **Ziffer 9**.

### Akzeptanzkriterien
- [ ] Weinende Katze ist in der Szene auffindbar (Audio/Animation).
- [ ] Drei Zutaten (Thunfisch, Reis, Milch) sind einsammelbar.
- [ ] „Kochen"-Aktion ist erst nach Sammeln aller 3 Zutaten möglich.
- [ ] Erfolgreiches Füttern liefert das Code-Fragment **Ziffer 9**.
- [ ] Feedback (Katze zufrieden / Animation).

### Aufgaben
- [ ] 3D-Assets Katze, Zutaten, Kochstation (`assets`)
- [ ] Inventar-/Sammel-Logik für die Zutaten
- [ ] Koch-/Füttern-Logik
- [ ] Lösungslogik + Übergabe der Ziffer an `CodeManager`

---

## Issue 5 — Rätsel 5/5: Code-Eingabe & Spielabschluss

**Labels:** `puzzle`, `gameplay`, `ui`, `MVP`
**Meilenstein:** MVP Level 1
**Bezug:** UC-04, UC-05, UC-06, Mock 5

### Beschreibung
Nachdem alle vier Ziffern gesammelt sind, gibt der Spieler den Gesamtcode **5792**
auf einem virtuellen Tastenfeld an der Tür ein. Bei korrekter Eingabe öffnet sich die
Tür und ein Glückwunsch-Screen erscheint, mit Option für nächstes Level oder Spielende.

### Akzeptanzkriterien
- [ ] `CodeManager` aggregiert die 4 Ziffern aus Rätsel 1–4.
- [ ] Virtuelles Tastenfeld zur Eingabe ist platziert und bedienbar.
- [ ] Eingabe **5792** öffnet die Tür (Animation), falsche Eingabe gibt Fehler-Feedback.
- [ ] Glückwunsch-/Abschluss-Screen wird angezeigt.
- [ ] Option „Nächstes Level" / „Beenden" vorhanden (Level-Übergang vorbereitet).

### Aufgaben
- [ ] Tastenfeld-UI (`ui`) + Eingabevalidierung
- [ ] Tür-Animation + Öffnungslogik
- [ ] Glückwunsch-Screen / Spielende
- [ ] `CodeManager`-Aggregation finalisieren

---

## Reihenfolge / Abhängigkeiten

```
Rätsel 1 (7) ─┐
Rätsel 2 (5) ─┤
Rätsel 3 (2) ─┼─► CodeManager (5792) ─► Rätsel 5 (Eingabe + Abschluss)
Rätsel 4 (9) ─┘
```

Die Rätsel 1–4 können weitgehend **parallel** entwickelt werden, sofern ein
gemeinsames Interface zur Ziffern-Übergabe (`CodeManager`) zuerst festgelegt wird.
Rätsel 5 hängt von allen vorherigen ab.
