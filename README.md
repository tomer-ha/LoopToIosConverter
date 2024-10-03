# Loop to iOS Converter

**NEW: try the [web-based converter](https://tomer-ha.github.io/LoopToIosConverter/)**

I am a long time [Loop Habit Tracker](https://github.com/iSoron/uhabits) user.
Recently I have decided to switch from Android phones to an iPhone, but I could not find good alternatives for habit-tracking.
It turns out that the good folks at Loop have already expressed a desire to create an iOS version, but it's still in the oven, possibly for a long time.

As an interim solution I decided to try another app from the iOS app store. It's called [Habit Tracker - Your Goals](https://apps.apple.com/us/app/habit-tracker-your-goals/id1471303896) and it resembles Loop closely.
Unfortunately, it is not open source and is ad-supported, but I can live with it for the time being. 

I didn't want to lose my data, so I made this little utility to convert data from Loop to the iOS counterpart, and back from iOS to Loop (hopefully Loop will become available for iOS or I'll return back to Android).

## Advanced Usage

Download the app and the [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download) for your machine (Windows, Linux and Mac are supported).

### Export from Android to iOS

- Go to Loop's settings page, choose "Export full backup" and send to your computer the ".db" file created (for example send yourself an email with the db file as attachment)
- Run: LoopToIosConverter.exe -i "<exported-backup-file.db>" -o "converted-file.csv"
- If you want, you can add "-s" to skip archived tasks or "-p" to preserve the order of your habits by adding "01. " prefixes (the iOS app doesn't have a concept of display order)
- Transfer the csv file to your iPhone (again, sending an email to yourself is a quick way - then save the csv file somewhere)
- Install [Habit Tracker - Your Goals](https://apps.apple.com/us/app/habit-tracker-your-goals/id1471303896) on your iPhone, go to the settings and choose "Import data"
- Locate the csv file you produced, wait a few seconds until you see a banner saying the import job was completed

### Export from iOS to Android

- Go to the iOS Habits app, enter the settings page an choose "Export data". This will use your iOS Mail app to send an email with the csv file attached. Send it to yourself and save a copy on your computer
- Run LoopToIosConverter.exe -i "<exported-backup-file.csv>" -o "converted-file.db"
- Transfer the db file to your Android phone (an email works just fine, then save the db file somewhere on your phone)
- Open the Loops Habits app (the icon says "Habits"), open the settings page and click "Import data". Choose the db file you produced

### Fix iOS to iOS backup

While working on this project, I discovered that the csv backup files produced by the iPhone app can't be fed back to the same app. 
This is because the csv file is CRLF terminated instead of just LF. Therefore, you can use the LoopToIosConverter tool to fix your backup file, by running:

LoopToIosConverter.exe -i "<exported-backup-file.csv>" -o "fixed-file.csv"
