# Show Tracking-TMDB

## Summary:

This project keeps track of Downloaded and Watched episodes of Shows. It also serves as a way to check for newly released epsiodes.
This utilizes the TMDB API to gather all information about a show.

## Functions: 

### Add:

Searches for a show based on user input. Then goes over the returned shows for the user to selected the show they were looking for.
The channel the show is associated can then be set, this can be skipped. The channel is purely for the user's benefit to remember where to watch the show.
The show's database table is then created and filled with every episode that has aired up to the date added.

### Remove:

Removes the show from the database.

### List Updatable:

Displays a table of shows with an updatable value of 'true'.

### Mark:

Allows the user to mark episodes as Watched or Downloaded. Setting an episode as Watched will also set it as Downloaded. 
This is done as having the episode Downloaded after being Watched seems unlikely to be utilized. 

Setting episodes as Watched can be achieved in two ways:
1. Season - Set all episodes of a season to Watched and Downloaded
2. Next - Chronologically displays episodes not set as Watched to the user. Selecting 'Y' sets the episode to Watched and Downloaded and then displays the next episode. Selecting 'N' exits Mark.

Setting episodes as Downloaded can be achieved in three ways:
1. Season - Set all episodes of a season to Downloaded
2. Next - Chronologically displays episodes not set as Downloaded to the user. Selecting 'Y' sets the episode to Downloaded and then displays the next episode. Selecting 'N' exits Mark.
3. Out Of Order - Allows the user to set episodes as Downloaded out of chrological order. User can select what season to start at and episodes not set as Downloaded will then be displayed chronologically. Selecting 'Y' or 'N' will continue to the next episode. Selection will end when the latest episode of the show has been set or the user enters 'cancel'.

### List Show:

Displays tables for each season of a selected show. Each row of the table represents an episode and provides the episode's number, title, Downloaded status, Watched status, and release date.

### Updatable

User chooses a show in the database and that show's updatable value will be set to 'true' if the value was initially 'false' or vice versa.

### Update:

For each show with an Updatable value of 'true', new episodes are searched for. The current latest season in the database is checked for more episodes and the next season is checked. If new episodes are found they are diplayed to the user and added to their respective show's table.

### Update Latest:

For a show selected by the user, the latest season in the database is effectively removed and recreated retaining any existing Watched and Downloaded settings on episodes. 

*Unsure if this is necessary. This is a holdover from the IMDb API.

### Single Update:

Runs the Update function but for only one user selected show.

### Mass Update:

Runs the Update function but ignores the Updatable value. The user selects a show and Alphabetically every subsequent show is updated until all remaining shows are updated or the API call limit is reached.

### Stop:

Ends the program.
