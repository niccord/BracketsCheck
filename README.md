# BracketsCheck
Notepad++ plugin written in C# for brackets balancing check.

It works on the sheet you are currently displaying.

This plugin was written with the goal of checking a large SQL file and then evolved a little.

If the brackets in the file are balanced, there's not a lot to say.

On the other hand, there are three cases when a bracket is not matched:
- a bracket opens and it never closes;
- a wrong type of bracket closes (a round one instead of a square one);
- a bracket closes with no opening correspondent.

For the first two scenarios, the plugin returns an error on the opening bracket.\
For the last scenario, it returns an error on the bracket itself which should be removed.

The type of brackets currently supported are:
- round ()
- square []
- curly {}
- angular <>

and each one of them can be disabled via the plugin settings menu.

### Version 1.2
Parameters are now saved and loaded
### Version 1.1
Added parameterization to decide which bracket type should be checked
