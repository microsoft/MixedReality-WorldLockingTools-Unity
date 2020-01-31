# Contributing to the World Locking Tools for Unity project

The most valuable way to contribute to the World Locking Tools project at this time is by filing issues. Any and all feedback on better aligning the World Locking Tools for Unity project with your project's needs is extremely valuable.

While any feedback you post is valuable, here are some tips on making yours more actionable.

## Use labels appropriately

Both when initially submitting an issue, and when following up on an issue as a contributor, proper use of labels is extremely helpful on coordinating with other contributors.

Try to differentiate accurately between what is a bug, what is a feature request, and what is a broader suggestion going forward. All are valuable, but they are more valuable once identified as such.

Likewise, if an issue seems unactionable in its current form, applying the appropriate label (e.g. "unclear") can help get it improved to where it is actionable. Specific comments in the issue itself are, of course, extremely valuable. But the proper label may lead others to see a comment that might otherwise go unnoticed.

## Reporting a bug

Issues may be submitted from the [issues portal](https://github.com/microsoft/MixedReality-WorldLockingTools-Unity/issues) on GitHub. Taking the time to report a problem or make a suggestion that others will benefit from as well is always appreciated.

Every bug report has its own context, but in general, the more of the following that are included the more quickly an issue can get resolved.

### Log files from the device


Log files from the device can be immeasurably helpful in investigating issues, especially in conjunction with screen captures suggested below. They may be obtained using the Windows Device Portal while connected to your device, under System > File explorer > User Folders \ LocalAppData \ WorldLockingTools

> #### The Unity app log file: 
>
> _UnityPlayer.log_ is located in the _TempState_ sub-folder. This is a plain text file.

> #### The World Locking Tools diagnostics recording:
>
> The diagnostics file is located in the _LocalState_ sub-folder. The file's name is auto-generated according to the following pattern:
>
> `FrozenWorld-<device name>-<capture date and time>.hkfw`
>
> It is a binary file which needs specialized software to examine.
> 
> Note that to capture a diagnostics recording requires enabling Diagnostics Recording on the World Locking Tools Manager component in your scene. See the [diagnostics](WorldLockingContext.md#diagnostics-settings) documentation for details.

### Repro steps

Specify how readily the issue happens. The ideal is having a bug that occurs 100% of the time following a certain set of step. But even for a bug that you've only seen happen once, the more detailed you can relate the steps leading up to the issue, the better.

Repro steps should follow the following general form:

1) Starting from this normal stable state...
2) Then I did this (or noticed this unusual thing)...
3) Then the system started showing this incorrect behavior... 

### Screen captures

Screen captures will help identify the full context the issue occurred in. In particular, having World Locking Tools diagnostics displayed on screen can help correlate your experience to the information in the logs. These can either be snapshot images, or video captures.

### Device info

* What type of device?
* Running what OS-version?

### Build environment

* Unity version
* Visual Studio version

## Proposing a feature

When you find that World Locking Tools _almost_ does what you need, the chances are that someone else is suffering the same limitation.  We are just as interested in fixing gaps in our documentation and examples as in providing new capabilities. 

In proposing a new feature, it's most valuable to make clear what it is you are trying to get done. While ideas on how to implement it can also be helpful, proposals that make clear the added value are more likely to gain traction. Make the problem the feature solves obvious, preferably with what you could accomplish with it in a real world scenario.

Make sure to attach the "enhancement" label to the submitted proposal issue.

## Contributing code

This is an open source project, so of course anyone can make a fork to develop on at any time. If someone is generous enough to share back work, then it is greatly appreciated, whether it gets folded back into the main repository or not.

During this initial roll out period, we will have limited resources to review and accept pull requests into the main repository. It is advisable to avoid investing a lot of time into a fork under the assumption that it will be merged back to the main repository.

One way to mitigate the risk is to submit an issue proposing what is intended (labeled "enhancement") before investing a lot of time in an implementation. This is also considerate toward other contributors that might be looking at the same problem area.

## See also

[Coding Conventions](CodingConventions.md)
[Release Process](ReleaseProcess.md)