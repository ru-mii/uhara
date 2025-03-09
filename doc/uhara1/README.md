### Setup
- Loads library and assigns an instance to the chosen variable.
- "Uhara" naming in "vars.Uhara" is optional and can be change to any other name for example "vars.FunStuff".
```c#
startup
{
    vars.Uhara = Assembly.Load(File.ReadAllBytes("Components/uhara1")).CreateInstance("Main");
}
```
------------------------------
