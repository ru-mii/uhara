## Syntax
```c#
void SetProcess(
[in] Process process
);
```   
## Parameters
**[in] process**   
A process of the game.   
## Return value
None
## Remarks
Assings passed Process object to Uhara and is then used for every operation that would require game's process.
In most cases would be used as a frist line in the init to pass the game's process grabbed by LiveSplit.   
```c#
init
{
    SetProcess(game);
}
```
This is not required as Uhara reads it with reflection but should be used if any compatibility issues arrive or in other edge cases.
