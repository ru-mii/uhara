## Syntax
```c#
void SetProcess(
[in] Process process
);
```
&nbsp;
&nbsp;
## Parameters
**[in] process**   
A process of the game.   
&nbsp;
&nbsp;
## Return value
Function does not return a value.
&nbsp;
&nbsp;
## Remarks
In most cases would be used as a frist line in the init to pass the game's process grabbed by LiveSplit.
```c#
init
{
  SetProcess(game);
}
```
Assings passed Process object to Uhara and is then used for every operation that would require game's process.
This is not required as Uhara reads it with reflection but should be used if any compatibility issues arrive or in other edge cases.
