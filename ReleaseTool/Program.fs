// Learn more about F# at http://fsharp.org

open System
open Hopac
open HttpFs.Client
open FSharp.Data
open dotenv.net

// JsonProvider uses example response to build the expected type
type ProjectResponse = JsonProvider<"""{
  "count": 16,
  "value": [
    {
      "id": "8ae3633b-b2c0-44a8-8457-fca4cfc7a43e",
      "name": "Infrastructure",
      "url": "https://dev.azure.com/metabolondev/_apis/projects/8ae3633b-b2c0-44a8-8457-fca4cfc7a43e",
      "state": "wellFormed",
      "revision": 495,
      "visibility": "private",
      "lastUpdateTime": "2019-09-26T14:29:19.16Z"
    }
  ]
}""">

let envOrNone key =
  match Environment.GetEnvironmentVariables().Contains(key) with
  | true -> Some (Environment.GetEnvironmentVariable(key))
  | false -> None

let getBaseDevOpsUrl project =
  sprintf "https://dev.azure.com/%s" project

let getProjects baseUrl token =
    let url = sprintf "%s/_apis/projects" baseUrl
    let requests =
      Request.createUrl Get url
      |> Request.basicAuthentication String.Empty token
      |> Request.responseAsString
      |> run
      |> ProjectResponse.Parse
    let projects = requests.Value
    projects |> Array.iter (printfn "%O")

[<EntryPoint>]
let main argv =
    DotEnv.Config()
    let devopsAccount = envOrNone "VSTS_ACCOUNT"
    let token = envOrNone "VSTS_TOKEN"
    match (devopsAccount, token) with
    | (None, None) -> printfn "Please provide env vars for VSTS_ACCOUNT, VSTS_TOKEN"
    | (None, _) -> printfn "Please provide env vars for VSTS_ACCOUNT"
    | (_, None) -> printfn "Please provide env vars for VSTS_TOKEN"
    | (d, t) ->
      let baseUrl = getBaseDevOpsUrl d.Value
      getProjects baseUrl t.Value
    0 // return an integer exit code
