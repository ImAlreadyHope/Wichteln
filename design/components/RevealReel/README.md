# RevealReel

The draw animation. A horizontal strip of participant avatars slides under a fixed pointer, decelerates, and stops on the person you drew; that avatar gets the `is-active` state (`gold` ring, `shadow-pop`).

How the app drives it:
- The server has already decided the result; the reel only shows it. Build the track as several shuffled loops of the other participants (never yourself) ending with the drawn person, so it passes everyone before landing.
- Animate `transform: translateX()` on `wi-reel__track` over about 2.6s with `cubic-bezier(.12,.8,.18,1)`, then add `is-active` to the final item.
- With `prefers-reduced-motion`, jump straight to the result.
- After landing, replace the reel and the button with the result: `wi-avatar--xl`, the name in `hero`, then their WishItem list. The button never returns.

Each item is 112px wide with a `space-4` gap; the reel is 180px tall on `surface-sunken` with `radius-xl` and soft fades at both ends.
