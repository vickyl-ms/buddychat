## Welcome to the buddy chat repo!

This project contains a CommandLine (CLI) utility for running a 1:1 buddy chat program for a group. For each round, you send out a signup form where people can either enter their info or opt into using their previous signup information.

## How to build
```dotnet build```

## How to run tests
```dotnet test```

## How to deploy
```dotnet publish```
Buddychatcli project is set to build for x64 as a standalone application

## How to use:
### 1. Create a signup form using https://forms.microsoft.com/ that gets name, email and various additional data fields. Download responses as csv files. This is an example of the kind of data you might want to collect in the signup form.

```
  "name": "Spike Spiegel",
  "email": "cowboy@bebop.com",
    "data": {
      "pronouns": "He / His",
      "introduction": "I was a former member of the Red Dragon Crime Syndicate, but I am now a bounty hunter and the partner of Jet Black, the captain of the Bebop",
      "question1": "What have you started since the pandemic?",
      "answer1": "Tennis with Jet",
      "question2": "What is the most unique trip you've gone on?",
      "answer2": "That time we went on Tarsis and found Ein."
    }
```
### 2. Create a signupsconfig.json file that describes how to translate from the excel response form from your signup survey into a Participant data structure as seen above.
```
{
    "emailIndex": 3,
    "nameIndex": 4,
    "dataEntries": [
        {
            "fieldName": "pronouns",
            "index": 7
        },
        {
            "fieldName": "introduction",
            "index": 8
        },
        {
            "fieldName": "question1",
            "index": 9
        },
        {
            "fieldName": "answer1",
            "index": 10
        }
    ]
}
```
### 3. Set up environment
Create env vars that specify path to previous data and new session name. Add cli to path.
```
set CURRENT_SESSION_NAME=211108
set PATH=%PATH%;<replace with path to buddychatcli.exe>

mkdir %CURRENT_SESSION_NAME%
pushd %CURRENT_SESSION_NAME%

REM If you have data from previous session, set macro and copy over config & email template files
set LAST_SESSION_PATH=..\211007

copy %LAST_SESSION_PATH%\signupsconfig.json
copy %LAST_SESSION_PATH%\emailtemplate.oft

```
### 4. Create or update the participants data file
If no dataEntries are present in the new signup information, it's assumed participant wanted to use previous signup data.
```
REM Choose a session name to use for generating files for each round of the chat program
REM The first time, you have no previous data
buddychatcli.exe updateParticipants -s %CURRENT_SESSION_NAME%

REM Once you have previous data, pass in the participant data file from the previous round
buddychatcli.exe updateParticipants -h %LAST_SESSION_PATH%\Participants.json -s %CURRENT_SESSION_NAME%
```
### 5. Create random pairings for people who signed up in a session making sure that they haven't buddied up in the past
```
REM Run from directory with the participants.json file from the previous step

REM The first time, you have no previous data
buddychatcli.exe CreatePairings -s %CURRENT_SESSION_NAME%

REM Once you have previous data, pass in the pairing history data file from the previous round
buddychatcli.exe CreatePairings -s %CURRENT_SESSION_NAME% -h %LAST_SESSION_PATH%\PairingHistory.json
```
### 6. Create or update the pairing history data file
```
REM Run from directory with the newpairings.json file from the previous step

REM The first time, you have no previous data
buddychatcli.exe UpdatePairingHistory

REM Once you have previous data, pass in the pairing history data file from the previous round
buddychatcli.exe UpdatePairingHistory -h %LAST_SESSION_PATH%\PairingHistory.json
```
### 7. \[optional\] Create an outlook email template (\*.oft) file with placeholders for your data
#### Example template:
```
🎺 We would like to introduce you to...

<participant1.name>                         <participant2.name>
<participant1.data.pronouns>                <participant2.data.pronouns>
<participant1.data.introduction>            <participant2.data.introduction>

<participant1.first_name>’s questions:      <participant2.first_name>’s questions:

<participant1.data.question1>               <participant2.data.question1>
<participant1.data.answer1>                 <participant2.data.answer1>

We borrowed a box-shaped randomness generator from our friend Schrödinger, and randomly determined that <participant1.name> should initiate the meeting. 

Please reach out to your assigned buddy this week and schedule a time (suggested 30 minutes) within the next two weeks to virtually meet each other and have a chat. 

(But <participant2.name>, if you don't see a meeting invite in 3 days, consider checking in with your buddy.)
```
#### To generate emails:
```
REM Run from directory with newpairings.json file
buddychatcli.exe CreateEmails -o emails
```
