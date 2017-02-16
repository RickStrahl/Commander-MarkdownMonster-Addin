# Build script that creates the \Build folder final output
# that can be shared on GitHub and the public Markdown Monster Addins Repo

cd "$PSScriptRoot" 

$src = "$env:appdata\Markdown Monster\Addins\Commander"
$tgt = "..\Build"
$dist = "..\Build\Distribution"

"Copying from: $src"

"Cleaning up build files..."
remove-item -recurse -force $tgt
md $tgt
md $dist

"Copying files for zip file..."
copy "$src\*.dll" $dist
copy ".\version.json" $dist

"Copying files for Build folder..."
copy ".\version.json" $tgt
copy ".\icon.png" $tgt
copy ".\screenshot.png" $tgt

"Zipping up setup file..."
7z a -tzip  $tgt\addin.zip $dist\*.*
