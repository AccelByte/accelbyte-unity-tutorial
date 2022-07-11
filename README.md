# Unity Tutorial Project

This project will help you to give an example of how to integrate the SDK into your current project. If you need a guide on how to configure Unity SDK into your current project, you can follow https://docs.accelbyte.io/initial-setup/sdk-guides/unity-sdk-getting-started.html.

## Current features implemented :

- Login
- MainMenu
- Lobby
- Friends
- Party
- Matchmaking
- Dedicated Server Integration
- Gameplay Integration

## Setup Project

1. Open the project using Unity Engine. (The recommendation version is using `2019.4` and don't use version `2020` for now.)
2. Click play to test the project.
3. If you don't have an account, you can register from the https://demo.accelbyte.io/register.

## Build Project 

### Build the Project using Batch File

The project can be built by using these steps.

1. In Editor, Open **Assets > Resources > AccelByteServerSDKConfig.json**.
2. Add the `ClientSecret` correctly or you can ask by contacting your Account Manager or support at [hello@accelbyte.io](mailto:hello@accelbyte.io).
3. Run the `BuildProject.bat`.
4. Input Engine and Project path. Wait until the process is done.

```
Tips: 
You can also configure the Engine and Project path manually by editing the BuildProject.bat.
```

### Build the Project Manually

The project can be built into the client and server.

#### - Client

1. In your Editor, go to **File > Build Settings**.
2. Uncheck the `Server Build` checkbox.
3. Click `Button` button to build the project as a client.

#### - Server

1. In Editor, Open **Assets > Resources > AccelByteServerSDKConfig.json**.
2. Add the `ClientSecret` correctly or you can ask by contacting your Account Manager or support at [hello@accelbyte.io](mailto:hello@accelbyte.io).
3. In your Editor, go to **File > Build Settings**.
4. Check the `Server Build` checkbox.
5. Click `Button` button to build the project as a server.

## How to Test the Game

### Test the Matchmaking using DS
1. Run `StartClientWithDS.bat`.
2. Input the Project path and wait until the process is done.

```
Tips: 
You can also configure the Project path manually by editing the StartClientWithDS.bat.
```

### Test the Matchmaking locally
1. Run `StartLocalClientWithLocalDS.bat`.
2. Input the Project path and wait until the process is done.

```
Tips: 
You can also configure the Project path manually by editing the StartLocalClientWithLocalDS.bat.
```

For information about these services, please contact [hello@accelbyte.io](mailto:hello@accelbyte.io).