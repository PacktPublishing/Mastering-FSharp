// Include Fake lib
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open System.IO

// Directories
let buildDir  = "./build/"

// version info
let version = "0.1"

let runExe projectDir =
    let projectName = Path.GetFileName(projectDir)
    let exePath = buildDir </> projectDir </> projectName + ".exe"
    ProcessHelper.directExec (fun info ->
        if EnvironmentHelper.isLinux
        then info.FileName <- "mono"; info.Arguments <- exePath
        else info.FileName <- exePath)
    |> ignore

let buildDebug projectDir =
    let projectName = Path.GetFileName(projectDir)
    let buildDir = buildDir </> projectDir
    CleanDir buildDir
    MSBuildDebug buildDir "Build" [projectDir </> projectName + ".fsproj"]
    |> ignore

Target "CompileAndRunTests" (fun _ ->
    // Clean and create build directory if it doesn't exist
    let buildDir = "build/tests"
    CleanDir buildDir
    CreateDir buildDir

    // COMPILE TESTS
    ["chapter-9/MyLibTests.fsx"]
    |> FscHelper.compile [
        // F# compiler options
        FscHelper.Out (buildDir </> "MyLibTests.dll")
        FscHelper.Target FscHelper.TargetType.Library
    ]
    // Raise exception if a code other than 0 is returned
    |> function 0 -> () | c -> failwithf "F# compiler return code: %i" c
    // Copy the NUnit assembly to the build directory
    FileUtils.cp "packages/NUnit/lib/net45/nunit.framework.dll" buildDir

    // RUN TESTS
    [buildDir </> "MyLibTests.dll"]
    |> Testing.NUnit3.NUnit3 (fun p ->
        {p with ToolPath="./packages/NUnit.ConsoleRunner/tools/nunit3-console.exe"})
)

Target "chapter-10/Chat.MailboxProcessor" (fun _ ->
    let projectDir = "chapter-10" </> "Chat.MailboxProcessor"
    buildDebug projectDir
    runExe projectDir
)

Target "chapter-10/Chat.Akka" (fun _ ->
    let projectDir = "chapter-10" </> "Chat.Akka"
    buildDebug projectDir
    runExe projectDir
)

Target "chapter-10/Chat.Akka.Supervision" (fun _ ->
    let projectDir = "chapter-10" </> "Chat.Akka.Supervision"
    buildDebug projectDir
    runExe projectDir
)

Target "chapter-10/Chat.Akka.Local" (fun _ ->
    let projectDir = "chapter-10" </> "Chat.Akka.Local"
    buildDebug projectDir
    runExe projectDir
)

Target "chapter-10/Chat.Akka.Remote" (fun _ ->
    let projectDir = "chapter-10" </> "Chat.Akka.Remote"
    buildDebug projectDir
    runExe projectDir
)

Target "Help" (fun _ ->
    printfn "Pass the chapter and the name of the project to run. Example:"
    printfn "build chapter-10/MailboxProcessor"
)

// start build
RunTargetOrDefault "Help"
