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
      "id": "2703b5c1-80fe-49dc-a657-25c3f50d5cc8",
      "name": "MyProject",
      "url": "https://dev.azure.com/myaccount/_apis/projects/2703b5c1-80fe-49dc-a657-25c3f50d5cc8",
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

let getBaseDevOpsUri project =
    let builder = UriBuilder("https://dev.azure.com/")
    builder.Path <- project
    builder.Uri

let getApiUri (baseUri: Uri) pathFragment =
    let builder = UriBuilder(baseUri)
    builder.Path <- builder.Path + pathFragment
    builder.Uri

let templateRequest (token: string) (makeUri: string -> Uri) pathFragment method : Request =
    let uri = makeUri pathFragment
    Request.createUrl method uri.AbsoluteUri
      |> Request.basicAuthentication String.Empty token

let getProjects (request: string -> HttpMethod -> Request) =
    let requests =
      request "/_apis/projects" Get
      |> Request.responseAsString
      |> run
      |> ProjectResponse.Parse
    let projects = requests.Value
    projects |> Array.iter (printfn "%O")

[<EntryPoint>]
let main argv =
    DotEnv.Config()
    match (envOrNone "VSTS_ACCOUNT", envOrNone "VSTS_TOKEN") with
    | (None, None) -> printfn "Please provide env vars for VSTS_ACCOUNT, VSTS_TOKEN"
    | (None, _) -> printfn "Please provide env vars for VSTS_ACCOUNT"
    | (_, None) -> printfn "Please provide env vars for VSTS_TOKEN"
    | (account, token) ->
      let baseUri = getBaseDevOpsUri account.Value
      let makeUri = getApiUri baseUri
      let request = templateRequest token.Value makeUri
      getProjects request
    0 // return an integer exit code
