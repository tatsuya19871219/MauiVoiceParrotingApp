# MauiVoiceParrotingApp

A toy program to parrot voice input with a certain time delay.

## Files edited

<details>
<summary>Details</summary>

 - MauiVoiceParrotingApp/
     - Converters/
         - [BusyStateToIndicatorColorConverter.cs](./MauiVoiceParrotingApp/Converters/BusyStateToIndicatorColorConverter.cs)
     - Platforms/
         - Android/
             - [AndroidManifest.xml](./MauiVoiceParrotingApp/Platforms/Android/AndroidManifest.xml)
             - [VoicePlayer.cs](./MauiVoiceParrotingApp/Platforms/Android/VoicePlayer.cs)
             - [VoiceRecorder.cs](./MauiVoiceParrotingApp/Platforms/Android/VoiceRecorder.cs)
         - Windows/
             - [VoiceParrotingService.cs](./MauiVoiceParrotingApp/Platforms/Windows/VoiceParrotingService.cs)
             - [VoicePlayer.cs](./MauiVoiceParrotingApp/Platforms/Windows/VoicePlayer.cs)
             - [VoiceRecorder.cs](./MauiVoiceParrotingApp/Platforms/Windows/VoiceRecorder.cs)
     - Services/
         - [VoiceDataSharedBuffer.cs](./MauiVoiceParrotingApp/Services/VoiceDataSharedBuffer.cs)
         - [VoiceParrotingService.cs](./MauiVoiceParrotingApp/Services/VoiceParrotingService.cs)
         - [VoicePlayer.cs](./MauiVoiceParrotingApp/Services/VoicePlayer.cs)
         - [VoiceRecorder.cs](./MauiVoiceParrotingApp/Services/VoiceRecorder.cs)
     - ViewModels/
         - [VoiceParrotingServiceStateViewModel.cs](./MauiVoiceParrotingApp/ViewModels/VoiceParrotingServiceStateViewModel.cs)
     - [MainPage.xaml](./MauiVoiceParrotingApp/MainPage.xaml)
     - [MainPage.xaml.cs](./MauiVoiceParrotingApp/MainPage.xaml.cs)
</details>

## UML (partial)

<details>
<summary>Class diagram (partial)</summary>

![Class Diagram](./uml/VoiceParroting.png)
</details>

<details>
<summary>Sequence diagram (Service construction)</summary>

![Sequence Diagram](./uml/VoiceParrotingSequence_ServiceConstruct.png)
</details>

<details>
<summary>Sequence diagram (Press Start button)</summary>

![Sequence Diagram](./uml/VoiceParrotingSequence_Start.png)
</details>

<details>
<summary>Sequence diagram (Press Cancel button)</summary>

![Sequence Diagram](./uml/VoiceParrotingSequence_Cancel.png)
</details>

## What I learnt from this project

- How to use audio devices
- How to request permissions (Android)
- How to use NAudio library (Windows)
- How to lock shared variables to access them correctly
- How to use value converters in binding
- How to get resource dictionary with C# script
