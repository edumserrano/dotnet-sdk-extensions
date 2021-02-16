# Code coverage on multiple projects using Coverlet

## Motivation

I want to get code coverage for all the tests in a solution. When I tried this, the information on how to do this is was not easy to discover as I expected and there were some gotchas that were not clear to me.

The information required to understand how to achieve this was scattered accross this links:

- [coverlet-coverage/coverlet](https://github.com/coverlet-coverage/coverlet)
- [Coverlet integration with MSBuild](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md)
- [coverlet/Documentation/Examples/MSBuild/MergeWith/HowTo.md](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Examples/MSBuild/MergeWith/HowTo.md)
- [Solution wide coverage #357](https://github.com/coverlet-coverage/coverlet/issues/357)
- [coverlet.msbuild 2.8.0 does not combine coverage when using MergeWith #678](https://github.com/coverlet-coverage/coverlet/issues/678#issuecomment-571212806)
- [Implement threshold per solution #598](https://github.com/coverlet-coverage/coverlet/issues/598#issuecomment-551174529)

## How to use

The example below shows how to get code coverage for all the projects in a solution. You need to add the **[coverlet.msbuild NuGet](https://www.nuget.org/packages/coverlet.msbuild/)** to the test projects.

```
dotnet test <insert here path the solution file> `
    --results-directory "$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results")" `
    --logger trx `
    /p:CollectCoverage=true `
    /p:CoverletOutput="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/")" `
    /p:MergeWith="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.json")" `
    /p:CoverletOutputFormat="json%2copencover" `
    -m:1
```

Notes on the above command:

- The command is set to run from a **powershell** shell. If you want to use a different shell you will have to adjust the command so that:  
  - it uses the correct multiline separator instead of powershell's one which is \`.
  - provide an absolute path to be used as the directory where the test results and merge with reports will be stored. In powershell we are using the `Join-Path -Path (Get-Location) ...` function to do this.
  - potentially update the `/p:CoverletOutputFormat` argument value to instead of using `%2` as a comma separator to actually use a comma `,`or the equivalent for your shell so that msbuild translates it to a comma.

- On the `/p:CoverletOutputFormat` argument the `%2` is as a comma separator. In powershell we have to do it like this as explained [here](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md#note-for-powershell--azure-devops-users).

- You **should** use an absolute directory for the `/p:MergeWith` argument. If you use a relative path then the path will be relative to each of csproj files for the test projects which means that when storing the code coverage results to merge from one test project to another the directory **might not be correct and cause an incorrect end result**. Unfortunately the docs example for [how to use merge with in msbuild does show the usage by using a relative path](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Examples/MSBuild/MergeWith/HowTo.md). **Do not use relative directories** as shown in that example unless you know what you are doing.

## Include or exclude results from code coverage

You can use filters to include or exclude results from code coverage using the flags `/p:Exclude` and `/p:Include`. For more info [see msbuild code coverage filters docs](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md#filters).

If you need to include/exclude multiple namespaces and you are running the command in powershell remember to use `%2` instead of `,` as a separator because of the reasons explained above. As an example:

```
/p:Include="[MyApp.Namespace1]*%2c[MyApp.Namespace2]*" `
```

will only include code coverage results for the two namespaces `MyApp.Namespace1` and `MyApp.Namespace2`.

## Get code coverage report

You need to install the [report generator dotnet tool](https://www.nuget.org/packages/dotnet-reportgenerator-globaltool). You can do it by running the following command:

```
dotnet tool install --global dotnet-reportgenerator-globaltool
```

After running the above command to get solution wide code coverage:

```
reportgenerator `
    "-reports:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.opencover.xml")" `
    "-targetdir:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/report")" `
    -reportTypes:htmlInline
```

You need to run the code coverage command first because the report uses the coverage files produced by that command. Notice that the `-reports` argument is set to the same directory used by the code coverage command.

## In short: run locally or on a CI server

The above commands can be run locally or on a CI server assuming you are using powershell and the working directory is set to the same folder where the solution project to test is.

1) Set the working directory to the folder where the solution to test is.
2) Get code coverage:

```
dotnet test <solution file name> `
    --results-directory "$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results")" `
    --logger trx `
    /p:CollectCoverage=true `
    /p:CoverletOutput="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/")" `
    /p:MergeWith="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.json")" `
    /p:CoverletOutputFormat="json%2copencover" `
    -m:1
```

3) Generate code coverage report:

```
reportgenerator `
    "-reports:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.opencover.xml")" `
    "-targetdir:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/report")" `
    -reportTypes:htmlInline
```

4) You can view the code coverage result by browsing to `-targetdir` directory from the report generator command and opening the `index.html` file.

### Note

If you are running the above commands and you do not clean previous test results than you might get incorrect coverage results. This is ussually not a problem on CI builds since you usually start from a clean state but if you're running locally consider cleaning the test results directory between runs.

Set the working directory to the folder where the solution to test is and execute the following:

```
rm -r tests/test-results
```