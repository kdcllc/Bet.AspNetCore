# Goolge ReCapture

The Design was to keep it as simple as possible.

Steps to enable this within your project

1. Add the following in `_ViewImports.cshtml`

```csharp
    @using Bet.AspNetCore.ReCapture
    @addTagHelper *, Bet.AspNetCore.ReCapture
```

2. Add In `Startup.cs` file

```csharp
   services.AddReCapture(Configuration);
```

3. On the form add this 

```html
    <google-recaptcha></google-recaptcha>
```