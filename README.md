# MCStatus
A minimal status checker library for Minecraft servers using the Server List Ping (SLP) interface in C#.

## Usage

The library provides a single method that allows you to get all the status information you need! (Well, at least as much as the Server List Ping interface provides.)

```cs
var status = await ServerListClient.GetStatusAsync("mc.hypixel.net");
Console.WriteLine($"{status.Players.Online} / {status.Players.Max} players are online!");
```

If the server is down or something else goes wrong an exception is thrown.

## Acknowledgments

Based on the amazing work by https://wiki.vg/!

## Issues?

Built primarily for personal use, but if you encounter any problems while using feel free to report them and I'll see what I can do!
