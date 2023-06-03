## What's Changed?

* Hotfix: fixed some crashes that would've prevented the creation of new moves

## Known Bugs

* Saving enhanced specials and rekkas is broken. The data itself except for the Special Data will be saved, but the move types themselves won't. It still exports fine, so progress on the game can still continue.
* Whenever you choose to export a file, it will export all of them at the same time. With the fact that saving enhanced specials is broken, this will easily result in accidentally creating broken json files. I suggest exporting the files when you are done with a character, deleting the other two from your git changes, and then pushing the one json file to the repo at a time.

### Any other issues?

Report an issue in the Trello board or open a [Github Issue](https://github.com/KnockoutArcade/Character-Data-Editor/issues/new)!
