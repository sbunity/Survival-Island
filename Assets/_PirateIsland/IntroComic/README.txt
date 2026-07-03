Comic cut assets for Unity UI

Canvas reference size: 864 x 1821 px.
Use Canvas Scaler -> Scale With Screen Size, Reference Resolution 864 x 1821, Match 0.5.

Each panel PNG has transparent corners/diagonal edges and should be placed according to comic_layout.json.
Suggested animation order is the file order: panel_01 through panel_07.

For Unity: create an empty RectTransform parent named ComicPage with size 864x1821.
Add each PNG as an Image child, preserve alpha, set pivot 0.5/0.5, sizeDelta and anchoredPosition from comic_layout.json.
Then animate each child CanvasGroup one by one.
