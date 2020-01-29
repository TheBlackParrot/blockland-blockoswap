function gatherChangelog() {
	%oldHighestRev = $Server::Crumbling::HighestRevision;
	$Server::Crumbling::HighestRevision = 0;

	%file = new FileObject();
	%file.openForRead("Add-Ons/Gamemode_Crumbling_Arena_Redux/changelog.txt");

	%revision = -1;
	%date = "";
	%count = 0;

	while(!%file.isEOF()) {
		%line = %file.readLine();

		if(getSubStr(%line, 0, 2) $= "r:") {
			%d = getSubStr(%line, 2, strLen(%line));
			
			%revision = getField(%d, 0);
			%date = getField(%d, 1);
			%count = 0;

			$Server::Crumbling::Revision[%revision, "date"] = %date;

			if(%revision > $Server::Crumbling::HighestRevision) {
				$Server::Crumbling::HighestRevision = %revision;
			}
			continue;
		}

		if(%line $= "NEXT") {
			$Server::Crumbling::Revision[%revision, "lineCount"] = %count;
			continue;
		}

		$Server::Crumbling::Revision[%revision, "line" @ %count] = %line;
		%count++;
	}

	%file.close();
	%file.delete();

	$Server::Name = "[r" @ $Server::Crumbling::HighestRevision @ "]" SPC $Pref::Server::Name;

	if(%oldHighestRev < $Server::Crumbling::HighestRevision) {
		for(%i = 0; %i < ClientGroup.getCount(); %i++) {
			serverCmdChangelog(ClientGroup.getObject(%i), $Server::Crumbling::HighestRevision);
		}
	}
}
gatherChangelog();

function serverCmdChangelog(%client, %revision) {
	if($Sim::Time - %client.lastChangelogCmd < 3) {
		return;
	}
	%client.lastChangelogCmd = $Sim::Time;

	%min = $Server::Crumbling::HighestRevision - 3;
	if(%min < 1) {
		%min = 1;
	}
	%max = $Server::Crumbling::HighestRevision;

	if(%revision !$= "") {
		%min = %max = %revision;
		if($Server::Crumbling::Revision[%revision, "lineCount"] $= "") {
			return;
		}
	}

	for(%i = %max; %i >= %min; %i--) {
		%client.chatMessage("\c4r" @ %i SPC "\c7--\c6" SPC $Server::Crumbling::Revision[%i, "date"]);
		for(%j = 0; %j < $Server::Crumbling::Revision[%i, "lineCount"]; %j++) {
			%line = $Server::Crumbling::Revision[%i, "line" @ %j];
			%tag = "";

			switch$(getSubStr(%line, 0, 2)) {
				case "b:":
					%tag = "\c0[Bugfix] ";
				case "n:":
					%tag = "\c2[New] ";
				case "c:":
					%tag = "\c3[Change] ";
			}

			messageClient(%client, '', %tag @ "\c6" @ getSubStr(%line, 2, strLen(%line)));
		}
	}
}