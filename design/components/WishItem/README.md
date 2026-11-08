# WishItem

One wish in a grouped list. The list (`ul.wi-list`) is the card; each `li.wi-item` is a row with a hairline `border` between rows.

Row parts, left to right:
- `wi-item__tile`: 40px `radius-sm` tile with the `gift` icon. Alternate `red` and `--green` tiles down the list.
- `wi-item__body`: `wi-item__title` (the wish, `body-strong`) and `wi-item__meta` (the link host as a `green-700` link, or "Kein Link").
- `wi-item__actions`: edit and delete ghost icon buttons on your own list; on your Wichtel's list, an `external` icon and the whole row links out.

The consumer provides the title, an optional URL (show only its host) and whether the list is editable. Long titles wrap.
