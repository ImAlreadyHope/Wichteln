Wichteln is a small Christmas gift-exchange app for one family: Justin, Luana, Lydia, Angelika and Denis. Each person opens a personal link, writes a wishlist, and on 20 November reveals whom they give a present to. The look is clean and calm like an Apple settings screen, warmed by Christmas red, pine green and a touch of gold, and by Kitti, the Siamese cat who guides every screen.

## Voice and copy

- Write in German, informal **du**, short and warm. "Trag ein, was du dir wünschst." not "Bitte geben Sie Ihre Wünsche ein."
- Buttons are verbs: **Zeige meinen Wichtel**, **Wunsch hinzufügen**, **Speichern**, **Link kopieren**.
- Name dates plainly: "Ab 20. November", "Noch 12 Tage".
- Kitti may speak in headings ("Kitti schläft noch", "Frohes Wichteln!"); body text stays plain. No emoji in the UI.
- Errors say what happened and what to do: "Dieser Link ist ungültig. Frag Denis nach deinem persönlichen Link."

## Color

- Page ground is `surface`; cards, inputs and lists sit on `surface-raised` with a 1px `border`.
- `red-500` is the brand color and fills the single primary action (`on-red` text). `green-500` fills secondary actions (`on-green` text). Use only `on-red` and `on-green` on those fills; they flip from white to near-black in dark mode.
- Red and green text use the deep `red-700` and `green-700`; links inside rows are `green-700`.
- Tinted cards use `red-100` or `green-100` with `ink` text. Alternate red and green so neighbouring tiles and avatars differ.
- `gold` is only a sparkle: stars, the countdown dot and the ring around the revealed person. `danger` is only for errors and delete, always with an icon or word.
- Both themes are designed: dark is a pine-green night with brighter red and green fills and dark text on them. Every text pair holds 4.5:1 in both themes; `border-strong` and `ring` hold 3:1.

## Type

- Display face **Fredoka** (`font-display`): `hero` for the reveal result, `title` for screen titles, `heading` for cards. Rounded and friendly, used sparingly.
- Text face **Nunito** (`font-sans`): `body` 16px for reading and inputs, `body-strong` for row titles, `callout` for helper text, `label` 800 weight for buttons and field labels, `caption` for badges and link hosts.
- Both ship as variable woff2 files in `fonts/`; self-host them, don't call Google Fonts at runtime.

## Shape, space and depth

- Corners are soft, never sharp: `radius-md` (14px) for buttons and inputs, `radius-lg` (20px) for cards and lists, `radius-xl` (28px) for the reveal stage and sheets, `radius-full` for avatars and badges.
- Spacing runs on a 4px base: `space-4` card padding and phone gutter, `space-6` between cards, `space-8` between sections.
- Depth is quiet: one `shadow-card` on raised cards, `shadow-pop` only on the revealed avatar. No bevels, no thick bottom borders, no gradients on controls.
- Layout is a single centred column, max 480px, like a phone app, on every screen size.

## Kitti, the mascot

Kitti is an original Siamese cat: a round head with a cream coat and seal-brown points on the ears, face mask, paws and tail, big pointed ears, huge glossy blue eyes, a little fang in a cheeky grin, a red Santa hat and a pine-green scarf. Six poses live in **Mascot**:

- `kitti-hello`: the welcome screen.
- `kitti-thinking`: loading screens.
- `kitti-sleep`: the locked reveal before 20 November, and the not-found page.
- `kitti-gift`: the unlocked reveal and empty wishlists ("Noch keine Wünsche").
- `kitti-celebrate`: while the reel spins.
- `kitti-wink`: on the result card ("Psst, nicht verraten!").

Masters are 1024px transparent PNGs in **Mascot**; the app ships 640px WebP copies in `wwwroot/img`.

Show at most one Kitti per screen, 120–200px wide, on `surface` or a tinted card. Don't recolor, crop or stretch her, and don't put text over her.

## Icons

Icons are drawn in-house on a 24px grid with a 2px round stroke (a Lucide-compatible style), stored in **Icons**. In code, inline the SVG with `class="wi-icon"` so it takes `currentColor`; the files themselves are inked in `ink`. Use 20px in buttons and tiles, 16px in badges and row meta.

## Components

All components are plain CSS classes in `components/bundle.css` with the `wi-` prefix, made to be used straight from Blazor markup: Button, Card, TextField, WishItem, Avatar, Badge, Notice, AppBar, Countdown and RevealReel. Load `tokens.css` first, then `bundle.css`, and put `class="wi-root"` on `body`.
