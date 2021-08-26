#!/bin/sh

# ---------------------------------------------
# Setup variables
# ---------------------------------------------
installPath="/opt/AstroBot"

sourcePath="./AstroBot/"
rootDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
RED='\033[0;31m'
GREEN='\033[1;32m'
PURPLE='\033[1;35m'
NC='\033[0m' # No Color
targetServiceFile="$installPath/AstroBot.service"
targetRunitFile="/etc/sv/AstroBot/run"
serviceUser="astrobot"


# ---------------------------------------------
# Build the repository
# ---------------------------------------------
echo -e "$PURPLE-----------------------------------------------------"
echo -e "Building AstroBot ..."
echo -e "-----------------------------------------------------$NC"

cd $sourcePath

dotnet restore
dotnet publish --configuration Release

# ---------------------------------------------
# Install
# ---------------------------------------------
echo -e "$PURPLE-----------------------------------------------------"
echo -e "Copying files ..."
echo -e "-----------------------------------------------------$NC"

echo -e "$GREEN Installing Astrobot into the folder '$installPath' $NC"

# Create install folder
sudo mkdir -p $installPath

# Copy the files
sudo rsync -rv bin/Release/netcoreapp3.1/publish/* $installPath/ --exclude appsettings.json

echo -e "$GREEN Setting up systemd service ...' $NC"

sudo cp $rootDir/AstroBot.service $targetServiceFile
echo "[Service]" | sudo tee --append $targetServiceFile
sudo echo "Type=simple" | sudo tee --append $targetServiceFile
sudo echo "User=$serviceUser" | sudo tee --append $targetServiceFile
sudo echo "WorkingDirectory=$installPath" | sudo tee --append $targetServiceFile
sudo echo "ExecStart=/bin/bash -c 'dotnet $installPath/AstroBot.dll'" | sudo tee --append $targetServiceFile
sudo echo "ExecStop=/bin/bash -c 'kill dotnet'" | sudo tee --append $targetServiceFile
sudo echo "Restart=on-failure" | sudo tee --append $targetServiceFile
sudo echo "" | sudo tee --append $targetServiceFile
sudo echo "[Install]" | sudo tee --append $targetServiceFile
sudo echo "WantedBy=multi-user.target" | sudo tee --append $targetServiceFile


echo -e "$GREEN Setting up runit service ...' $NC"
sudo mkdir -p /etc/sv/AstroBot/
sudo cp $rootDir/runitService $targetRunitFile
sudo chmod +x $targetRunitFile
sudo echo "cd $installPath" | sudo tee --append $targetRunitFile
sudo echo "exec chpst -u $serviceUser /usr/share/dotent/dotnet $installPath/AstroBot.dll" | sudo tee --append $targetRunitFile


# Create symlink
sudo ln -sf $targetServiceFile /etc/systemd/system/AstroBot.service
sudo systemctl daemon-reload
sudo chmod +x $installPath/StartAstroBot.sh

# ---------------------------------------------
# Create user
# ---------------------------------------------
if getent passwd $serviceUser > /dev/null 2>&1; then
    echo -e "$GREEN User $serviceUser does already exist $NC"
else
    echo -e "$GREEN Creating user $serviceUser $NC"

    # Create user with random pasword
    randompw=$(cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 8 | head -n 1)
    sudo useradd $serviceUser
    echo $serviceUser:$randompw | chpasswd

    sudo usermod -u 920 $serviceUser     # Change user id to a system account, so it does not show up on display managers
fi

# Set permissions
sudo chown -R $serviceUser:$serviceUser $installPath
sudo chmod -R 755 $installPath
