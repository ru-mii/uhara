### Setup
Loads library and assigns an instance to vars.Uhara.
```c#
startup
{
    Assembly.Load(File.ReadAllBytes("Components/uhara1")).CreateInstance("Main");
}
```
Functions would then be used with the var, for example:
```
vars.Uhara.ExampleFunction(5);
```
