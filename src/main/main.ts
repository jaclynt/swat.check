import {app, BrowserWindow, ipcMain, session, Menu, dialog, shell} from 'electron';
import {join, dirname} from 'path';
import fs from 'fs';

import windowStateKeeper from 'electron-window-state';
import parseArgs from 'electron-args';
import Store from 'electron-store';

const store = new Store();
let DEV_MODE = process.env.NODE_ENV === 'development';
let mainWindow:BrowserWindow;

const cli = parseArgs(`
		swatcheck
 
		Usage
			$ swatcheck [path_to_project]
 
		Options
			--help                      show help
			--version                   show version
 
		Examples
			$ swatcheck path/to/project
`, {
		alias: {
			h: 'help'
		}
});
global.project_path = !DEV_MODE ? cli.input[0] : null;

function createWindow () {
	let mainWindowState = windowStateKeeper({
		defaultWidth: 1200,
		defaultHeight: 800
	});

	mainWindow = new BrowserWindow({
		width: mainWindowState.width, 
		height: mainWindowState.height,
		x: mainWindowState.x,
		y: mainWindowState.y,
		title: 'SWAT Check',
		icon: join(app.getAppPath(), 'static/256x256.png'),
		webPreferences: {
			preload: join(__dirname, 'preload.js'),
			nodeIntegration: false,
			contextIsolation: true,
		}
	});

	mainWindowState.manage(mainWindow);

	if (process.env.NODE_ENV === 'development') {
		const rendererPort = process.argv[2];
		mainWindow.loadURL(`http://localhost:${rendererPort}`);
	}
	else {
		mainWindow.loadFile(join(app.getAppPath(), 'renderer', 'index.html'));
	}
}

app.whenReady().then(() => {
	createWindow();

	session.defaultSession.webRequest.onHeadersReceived((details, callback) => {
		callback({
			responseHeaders: {
				...details.responseHeaders,
				'Content-Security-Policy': ['script-src \'self\'']
			}
		})
	})

	app.on('activate', function () {
		// On macOS it's common to re-create a window in the app when the
		// dock icon is clicked and there are no other windows open.
		if (BrowserWindow.getAllWindows().length === 0) {
			createWindow();
		}
	});
});

app.on('window-all-closed', function () {
	if (process.platform !== 'darwin') app.quit()
});

ipcMain.on('message', (event, message) => {
	console.log(message);
})