if(!isObject(BlockoswapLeaderboard)) {
	new GuiTextListCtrl(BlockoswapLeaderboard);
	$Server::Blockoswap::LoadData = 1;
}

function BlockoswapLeaderboard::getScore(%this, %client) {
	%data = %this.getRowTextByID(%client.bl_id);

	if(%data $= "") {
		return 0;
	}
	return getField(%data, 1);
}

function BlockoswapLeaderboard::updateScore(%this, %client, %score) {
	%data = %this.getRowTextByID(%client.bl_id);

	if(%data $= "") {
		%this.addRow(%client.bl_id, %client.getPlayerName() @ "\t" @ (%score || 0) @ "\t" @ %client.bl_id);
	} else {
		%this.setRowByID(%client.bl_id, %client.getPlayerName() @ "\t" @ (%score || 0) @ "\t" @ %client.bl_id);
	}

	%this.sortNumerical(1, 0);
}

function BlockoswapLeaderboard::saveData(%this) {
	%filename = "config/server/Blockoswap/leaderboard.db";

	%file = new FileObject();
	%file.openForWrite(%filename);

	for(%i=0;%i<%this.rowCount();%i++) {
		%file.writeLine(%this.getRowText(%i));
	}

	%file.close();
	%file.delete();

	export("$Server::BlockoswapStats*", "config/server/Blockoswap/stats.cs");
}

function BlockoswapLeaderboard::loadData(%this) {
	%filename = "config/server/Blockoswap/leaderboard.db";

	%file = new FileObject();
	%file.openForRead(%filename);

	while(!%file.isEOF()) {
		%line = %file.readLine();

		%id = getField(%line, 2);
		%score = getField(%line, 1);

		if(%id $= "" || %score $= "") {
			continue;
		}

		%this.addRow(%id, %line);
	}

	%file.close();
	%file.delete();

	%this.sortNumerical(1, 0);
}

function serverCmdLeaderboard(%client) {
	if(!isObject(BlockoswapLeaderboard)) {
		return;
	}
	%list = BlockoswapLeaderboard;

	if($Sim::Time - %client.lastLeaderboardCmd <= 2) {
		return;
	}
	%client.lastLeaderboardCmd = $Sim::Time;

	%count = %list.rowCount();
	if(%count > 15) {
		%count = 15;
	}

	if(!%count) {
		return;
	}

	for(%i = 0; %i < %count; %i++) {
		%row = %list.getRowText(%i);
		messageClient(%client, '', "\c3" @ %i+1 @ ". \c6" @ getField(%row, 0) SPC "-\c4" SPC getField(%row, 1));
	}

	messageClient(%client, '', "\c6--------");

	%pos = %list.getRowNumByID(%client.bl_id);
	for(%i = (%pos-2); %i <= (%pos+2); %i++) {
		if(%i < 0 || %i > %list.rowCount()-1) {
			continue;
		}

		%row = %list.getRowText(%i);

		if(%pos == %i) {
			messageClient(%client, '', "\c3" @ %i+1 @ ". \c2" @ getField(%row, 0) SPC "-\c5" SPC getField(%row, 1));
		} else {
			messageClient(%client, '', "\c3" @ %i+1 @ ". \c6" @ getField(%row, 0) SPC "-\c5" SPC getField(%row, 1));
		}
	}
}

if($Server::Blockoswap::LoadData !$= "") {
	BlockoswapLeaderboard.loadData();
	$Server::Blockoswap::LoadData = "";
}

function testGradient(%col1, %col2, %col3, %max) {
	for(%i = 0; %i < %max; %i++) {
		if(%i > mFloor(%max/2)) {
			%endCol = %col3;
			%startCol = %col2;
			%weight = mClampF((%i-(%max/2))/(%max/2), 0, 1);

			%col = _BS_RGBToHex(interpolateColor(%startCol, %endCol, %weight, 1));
		} else {
			%endCol = %col2;
			%startCol = %col1;
			%weight = mClampF(%i/(%max/2), 0, 1);

			//talk(%weight);

			%col = _BS_RGBToHex(interpolateColor(%startCol, %endCol, %weight, 1));
		}		

		messageAll('', "<color:" @ %col @ ">" @ %col);
	}
}

package BlockoswapTagsPackage {
	function serverCmdMessageSent(%client, %msg) {
		%list = BlockoswapLeaderboard;
		%row = %list.getRowNumByID(%client.bl_id);
		if(%row == -1) {
			return parent::serverCmdMessageSent(%client, %msg);
		}
		%row++;

		%count = %list.rowCount();
		%col = "FFFFFF";

		if(%row > mFloor(%count/2)) {
			%endCol = "255 255 255";
			%startCol = "0 160 255";
			%weight = mClampF((%row-(%count/2))/(%count/2), 0, 1);

			%col = _BS_RGBToHex(interpolateColor(%startCol, %endCol, %weight, 1));
		} else {
			%endCol = "0 160 255";
			%startCol = "0 255 0";
			%weight = mClampF(%row/(%count/2), 0, 1);

			//talk(%weight);

			%col = _BS_RGBToHex(interpolateColor(%startCol, %endCol, %weight, 1));
		}

		%client.clanPrefix = "\c7[<color:" @ %col @ ">" @ getPositionString(%row) @ "\c7]" SPC %client.originalPrefix;
		return parent::serverCmdMessageSent(%client, %msg);
	}
};
activatePackage(BlockoswapTagsPackage);