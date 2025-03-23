### Setup
Loads a library, in the backend the code assigns an instance to vars.Uhara
```c#
startup
{
    Assembly.Load(File.ReadAllBytes("Components/uhara2")).CreateInstance("Main");
}
```
Functions would then be used with the var, for example:
```
vars.Uhara.ExampleFunction(5);
```
