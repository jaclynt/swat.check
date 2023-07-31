# README #

Desktop interface for SWAT Check for SWAT2012. Download at [swat.tamu.edu/software/swat-check/](https://swat.tamu.edu/software/swat-check/).

## Installing and running the source code ##

### Back-end development stack ###

1. Install [.NET 7 SDK](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net70)
    * You may use [Visual Studio](https://visualstudio.microsoft.com/downloads/) or, [Visual Studio Code](https://code.visualstudio.com/) with .NET CLI commands

### Front-end development stack ###

1. Install [Node.js](https://nodejs.org/en/) (version 18 LTS)
2. Install required Node.js packages
    * From command prompt, go to the root directory of the source code
    * Run `npm install`

### Running the source code ###

1. From command prompt, go to the root directory of the source code
2. Run `npm run dev`

### Build the source code ###

1. From `/src/api` publish the .NET project to the `/src/main/static/api_dist` directory
2. From the root of the source code directory run the following:
	* Windows: `npm run build:win`
3. Program will be in `/release/dist`
