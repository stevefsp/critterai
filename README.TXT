>> General <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


The main directory structure is broken down by project then platform/language.

For example: 

The java utility project is under util/java.
The Unity3D utility project is under util/u3d.

Individual distributions are structured as follows:

project directory
    planform directory
        src directory   : Project source.
        test directory  : Test source.



>> Java Specific <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


Ant for builds.
JUnit for testing.



>> .NET Specific <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


No automatic builds or packaging.
Visual Studio Test for testing.
Sandcastle for documentation build.

If a project depends on a shared CritterAI library, then the following SVN
directory must be available: \lib\dotNet

DOCUMENTATION BUILD NOTES

The following tools are required to build the API documentation:

Sandcastle: http://sandcastle.codeplex.com/
Sandcastle Help File Builder: http://shfb.codeplex.com/

Sandcastle Styles Patch

http://sandcastlestyles.codeplex.com/ - Home Page
http://sandcastlestyles.codeplex.com/releases/view/47767 - Patch Used

Even with the above patches the "Send Feedback" functionality is not compatible
with CritterAI feedback method.  Until an automated process is implemented, 
the problem is being handled using post-processing in Dreamweaver.

Search for:

(The search is different for each project.  Search for "Send Feedback" to locate
the exact search phrase to use.)

<a href="javascript:SubmitFeedback('','CritterAI Navigation Library (C#/Unity3D)','','','','%0\dYour%20feedback%20is%20used%20to%20improve%20the%20documentation%20and%20the%20product.%20Your%20e-mail%20address%20will%20not%20be%20used%20for%20any%20other%20purpose%20and%20is%20disposed%20of%20after%20the%20issue%20you%20report%20is%20resolved.%20%20While%20working%20to%20resolve%20the%20issue%20that%20you%20report,%20you%20may%20be%20contacted%20via%20e-mail%20to%20get%20further%20details%20or%20clarification%20on%20the%20feedback%20you%20sent.%20After%20the%20issue%20you%20report%20has%20been%20addressed,%20you%20may%20receive%20an%20e-mail%20to%20let%20you%20know%20that%20your%20feedback%20has%20been%20addressed.%0\A%0\d','Customer%20Feedback');">Send Feedback</a>

Replace with:

<a href="http://www.critterai.org/contact" target="_blank">Send Feedback</a>



>> Unity Specific <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


Projects targeted to the 'u3d' platform depend on Unity (http://unity3d.com/).
Some require Unity Pro.  See project home for details. (http://critterai.org/)

The Visual Studio projects were created on a 64-bit version of Windows, so the
Unity DLL reference may be broken on 32-bit Windows.

For Unity 3.0, the expected location of the DLL for both operating systems is 
as follows:

Windows 32-bit: C:\Program Files\Unity\Editor\Data\Managed
Windows 64-bit: C:\Program Files (x86)\Unity\Editor\Data\Managed\


