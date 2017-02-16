# Build script that creates the \Build folder final output
# that can be shared on GitHub and the public Markdown Monster Addins Repo

cd "$PSScriptRoot" 

$src = "."
$dlls = "$env:appdata\Markdown Monster\Addins\Commander"
$tgt = "..\Build"
$dist = "..\Build\Distribution"

"Copying from: $dlls"

"Cleaning up build files..."
remove-item -recurse -force $tgt
md $tgt
md $dist

"Copying files for zip file..."
copy "$dlls\*.dll" $dist
copy "$src\version.json" $dist

"Copying files for Build folder..."
copy "$src\version.json" $tgt
copy "$src\icon.png" $tgt
copy "$src\screenshot.png" $tgt

"Zipping up setup file..."
7z a -tzip  $tgt\addin.zip $dist\*.*
