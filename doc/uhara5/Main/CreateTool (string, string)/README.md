## Syntax
```c#
IntPtr CreateTool(
[in] string grand
[in] string sub
);
```   
## Parameters
**[in] grand**   
Broad tool name.   
<br>
**[in] sub**   
Sub category of the tool.
## Return value
Returns object with the type of created tool.   
Returns null if failed or not found.   
## Remarks
Has to be used to use specific tool functions.   
Extracts required library dependencies to LiveSplit folder
