### Setup
- Loads library and assigns an instance to the chosen variable
- "Uhara" naming in "vars.Uhara" is optional and can be changed to any other name for example "vars.FunStuff"
```c#
startup
{
    vars.Uhara = Assembly.Load(File.ReadAllBytes("Components/uhara1")).CreateInstance("Main");
}
```
Functions would then be used with the created var, for example:
```
vars.Uhara.ExampleFunction(5);
```
