# Security considerations on GitHub workflows regarding dotnet CLI

## MSBuild and malicious pull requests

When thinking about malicious pull requests one wants to prevent the code from the malicous pull request gaining access to any sensible information on the CI system. More often than not this doesn't mean code that is added to the source code of the project, for instance adding a malicous class into a `.NET` project, but code that is added to repo which can be executed as part of the workflow running in CI.

Usually these come in the form of scripts which you can manipulate to gain uninteded access. Some could think that because they are using `dotnet`, they don't have to worry about this. However this is not true.

When running commands with the `dotnet` CLI you need to remember that some of them are backed by `MSBuild` which is extensible and as such can provide attack vectors for malicous attacks. For instance, when running `dotnet build`, [MSBuild is used to build the project](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build#msbuild), which means you can [add targets](https://docs.microsoft.com/en-us/visualstudio/msbuild/how-to-extend-the-visual-studio-build-process) to a `csproj` to let you execute arbitrary commands before or after the build process. In fact, there are many extension points which can be abused by malicious actors.

## If I restrict the permissions when building code then I'm safe right?

Not quite. As detailed in [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md), one must be sure that code from a potetially malicous actor never gets executed in a priviliged context.

So even if you are not building the code, you might be exposed. For isntance:

- create a GitHub workflow that runs when the trigger is `pull_request_target`.
- checkout the incoming code, meaning the code from the source repo, which in this scenario is the potentially malicious code.
- execute `dotnet format` just to check if the code guidelines are as expected.

There's nothing wrong with the above is there? Well you could immediatly say that we shouldn't be checking out code from a potentially malicious source when running in the priviliged context that `pull_request_target` gives us, however, if we are not going to execute that code what is the harm ?

Usually you would be correct. If you are in a priviliged context but don't execute code then you are still safe. However, `dotnet format` executes `MSBuild` tasks which given the extensibility explained above puts us in an exposed context. How can we be exposed? Consider the following task:

```
<Target Name="CustomTask" BeforeTargets="restore">
    <Message Importance="high" Text="THIS GETS CALLED FROM DOTNET FORMAT" />
    <!-- You could use the Exec task to run any command, for instance to run a powershell script -->
    <!-- https://docs.microsoft.com/en-us/visualstudio/msbuild/exec-task -->
</Target>
```

The above task will be executed when `dotnet format` runs, because the format command will restore `NuGet` packages at the start and since you can run arbitrary commands, it exposes your workflow. A malicious actor could add a script that would extract sensitive information from your workflow as part of a task like the above.

For completeness here is how the output from `dotnet format` looks like on a project with the above task:

```
The dotnet runtime version is '6.0.1'.
Formatting code files in workspace 'D:\Dev\edumserrano\dotnet-sdk-extensions\src\DotNet.Sdk.Extensions.Testing\DotNet.Sdk.Extensions.Testing.csproj'.
  Determining projects to restore...
THIS GETS CALLED FROM DOTNET FORMAT
Restored D:\Dev\edumserrano\dotnet-sdk-extensions\src\DotNet.Sdk.Extensions.Testing\DotNet.Sdk.Extensions.Testing.csproj (in 299 ms).
Project DotNet.Sdk.Extensions.Testing(net6.0) is using configuration from 'D:\Dev\edumserrano\dotnet-sdk-extensions\.editorconfig'.
Project DotNet.Sdk.Extensions.Testing(net6.0) is using configuration from 'D:\Dev\edumserrano\dotnet-sdk-extensions\src\DotNet.Sdk.Extensions.Testing\obj\Debug\net6.0\DotNet.Sdk.Extensions.Testing.GeneratedMSBuildEditorConfig.editorconfig'.
Project DotNet.Sdk.Extensions.Testing(net6.0) is using configuration from 'C:\Program Files\dotnet\sdk\6.0.101\Sdks\Microsoft.NET.Sdk\analyzers\build\config\analysislevel_6_all.editorconfig'.
Running 76 analyzers on DotNet.Sdk.Extensions.Testing(net6.0).
Running 507 analyzers on DotNet.Sdk.Extensions.Testing(net6.0).
Formatted 0 of 147 files.
Format complete in 10203ms.
```

Notice the `THIS GETS CALLED FROM DOTNET FORMAT` message just after the `Determining projects to restore...` message. This message comes from the `CustomTask` shown above.

## How to be secure

The solution is the same explained in [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md#How-to-be-secure). Make sure you do NOT execute potentially malicious code in a priviliged context.

Let's revisit the example given about `dotnet format`. Let's say what you want do to is run `dotnet format` when a pull request is made and automatically push any changes detected from `dotnet format` to the Pull Request. The issue here is that if you want to be secure you cannot simultaneous checkout and execute the incoming code and give the workflow permissions that you need to push code to the Pull Request, permissions such as a token with write permissions to the repo.

In this case what you can do is:

- run `dotnet format` in a restricted context. For instance by using the `pull_request` trigger. This way if a malicious actor tries to execute some exploit the workflow won't have the permissions required.
- upload the code that was formatted as a workflow artifact.
- trigger a separate workflow once the `dotnet format` worlfow completes.
- the new workflow will download the artifact from the `dotnet format` workflow and push the code changes to the repo. This workflow will run in a priviliged context but won't execute any of the malicious code so it is safe.
