<script setup lang="ts">
	import { reactive } from 'vue'
	const electron = window.electronApi;

	let globals:any = {
		dev_mode: true,
		platform: null,
		project_path: null,
		version: null
	}

	globals = electron.getGlobals();
	electron.setWindowTitle(`SWAT Check v${globals.version}`);
	
	let page:any = reactive({
		errors: false,
		projectPath: globals.project_path,
		readFiles: true,
		tab: 'setup',
		isReady: false,
		isRunning: false,
		hasError: false
	})

	let status:any = reactive({
		progress: 0,
		message: '',
		exception: null,
		data: null,
		runTime: null
	})

	let data:any = {
		setup: {},
		hydrology: {},
		nitrogenCycle: {},
		phosphorusCycle: {},
		plantGrowth: {},
		landscapeNutrientLosses: {},
		landUseSummary: {},
		pointSources: {},
		reservoirs: {},
		instreamProcesses: {},
		sediment: {}
	}

	function getTabClass(menuItem:string):string {
		if (menuItem === page.tab) return 'nav-link active';
		return 'nav-link text-white';
	}

	function setPageTab(selected:string):void {
		page.tab = selected;
	}

	function selectDirectory():void {
		var files = electron.openFileDialog({properties: ['openDirectory']});
		if (files !== undefined) {
			page.projectPath = files[0];
		}
	}

	function runSwatCheck():void {
		page.isRunning = true;
	}

	function cancelRun():void {
		page.isRunning = false;
		page.isReady = false;
		page.hasError = false;
	}
</script>

<template>
	<div id="app">
		<main v-if="page.isRunning">
			<div class="p-5 text-center w-75 mx-auto">
				<h1 class="h3">Running SWAT Check</h1>

				<div class="progress" role="progressbar" aria-label="SWAT Check Progress" :aria-valuenow="status.progress" aria-valuemin="0" aria-valuemax="100">
					<div class="progress-bar progress-bar-striped progress-bar-animated" :style="`width: ${status.progress}%`"></div>
				</div>

				<div class="my-3">
					{{ status.message }}
				</div>

				<div class="my-3">
					<a href="#" @click="cancelRun" class="text-danger">Cancel</a>
				</div>
			</div>
		</main>
		<main v-else-if="page.hasError">
			<div class="p-5 w-75 mx-auto">
				<h1 class="h3 text-center">Error Running SWAT Check</h1>

				<div class="alert alert-warning lead" role="alert">
					{{ status.message }}
				</div>

				<p>
					Please read through the message and stack trace carefully in case you
					can fix the error on your own. If unsure the cause of the error, 
					copy and paste the text below into the <open-in-browser url="http://groups.google.com/group/swatuser" text="SWAT User Support Group" />.
				</p>

				<div class="my-3">
					<textarea class="form-control" rows="10" readonly>SWAT Check v{{ globals.version }}
{{ status.exception }}</textarea>
				</div>

				<div class="my-3">
					<button @click="cancelRun" type="button" class="btn btn-lg btn-success">Clear Error and Return to Setup</button>
				</div>
			</div>
		</main>
		<main v-else class="d-flex flex-nowrap">
			<div class="d-flex flex-column flex-shrink-0 p-3 text-bg-dark" style="width: 280px;">
				<div class="fs-4">SWAT Check</div>
				<hr />
				<ul class="nav nav-pills flex-column mb-auto">
					<li class="nav-item"><a href="#" @click="setPageTab('setup')" :class="getTabClass('setup')">Setup</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('hydrology')" :class="getTabClass('hydrology')">Hydrology</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('sediment')" :class="getTabClass('sediment')">Sediment</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('nitrogenCycle')" :class="getTabClass('nitrogenCycle')">Nitrogen Cycle</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('phosphorusCycle')" :class="getTabClass('phosphorusCycle')">Phosphorus Cycle</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('plantGrowth')" :class="getTabClass('plantGrowth')">Plant Growth</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('landscapeNutrientLosses')" :class="getTabClass('landscapeNutrientLosses')">Landscape Nutrient Losses</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('landUseSummary')" :class="getTabClass('landUseSummary')">Land Use Summary</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('instreamProcesses')" :class="getTabClass('instreamProcesses')">Instream Processes</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('pointSources')" :class="getTabClass('pointSources')">Point Sources</a></li>
					<li class="nav-item"><a href="#" @click="setPageTab('reservoirs')" :class="getTabClass('reservoirs')">Reservoirs</a></li>
				</ul>
				<hr />
				<div>
					<a href="#" @click="setPageTab('help')" class="text-white text-decoration-none"><font-awesome-icon :icon="['fas', 'question-circle']" fixed-width /> Help</a>
				</div>
			</div>

			<div id="content-window" class="py-3 px-4 flex-grow-1">
				<div v-if="page.tab === 'setup'">
					<h1 class="h3 mb-3">Run SWAT Check</h1>

					<div class="bg-light p-3 mb-4">
						
						<div class="mb-3">
							<label for="projectPath" class="form-label">Project location (typically Scenarios/Default/TxtInOut)</label>
							<div class="input-group">
								<input id="projectPath" type="text" class="form-control" v-model="page.projectPath" />
								<button class="btn btn-primary" type="button" @click="selectDirectory">Browse</button>
							</div>
						</div>
						<div class="form-check mb-3">
							<input class="form-check-input" type="checkbox" id="readFiles" v-model="page.readFiles">
							<label class="form-check-label" for="readFiles">
								Read SWAT output files into SQLite?
								<br />
								<em>If you have already run SWAT Check on this project and the model hasn't changed/run again, 
								you may save time by unchecking this box.</em>
							</label>
						</div>

						<div class="mt-4 mb-3">
							<button class="btn btn-success btn-lg" type="button" @click="runSwatCheck">Examine Model Output</button>
						</div>
					</div>

					<p>
						SWAT Check reads model output from a SWAT project and performs many simple checks to identify
						potential model problems. The intended purpose of this program is to identify model problems early 
						in the modeling process. Hidden model problems often result in the need to recalibrate or regenerate a model, 
						resulting in an avoidable waste of time. This program is designed to compare a variety of SWAT outputs to 
						nominal ranges based on the judgment of model developers. A warning does not necessarily indicate a problem; 
						the purpose is to bring attention to unusual predictions. This software also provides a visual representation 
						of various model outputs to aid novice users.
					</p>
					<p>
						<span class="text-danger">WARNING:</span> SWAT Check must have write-access to your SWAT project directory. For large, daily output models, 
						reading SWAT's output.rch file may take a while (for example, if output.rch is over 200MB).
					</p>
				</div>

				<div v-else-if="page.tab === 'help'">
					<h1 class="h3">Help with SWAT Check</h1>

					<p>
						This version of SWAT Check has been tested with <b>SWAT 2012 revisions up to 688</b>. 
						SWAT 2009 revisions 500 or earlier may not run properly. This version is also NOT compatible with SWAT+.
						For SWAT+, SWAT+ Check is included with <open-in-browser url="https://swat.tamu.edu/software/plus" text="SWAT+ Editor" />.
					</p>

					<ul class="list-group list-group-flush border-top border-bottom">
						<li class="list-group-item"><open-in-browser url="https://swat.tamu.edu/software/swat-check/" text="SWAT Check Website" /></li>
						<li class="list-group-item"><open-in-browser url="https://swat.tamu.edu/" text="SWAT Website" /></li>
						<li class="list-group-item"><open-in-browser url="http://groups.google.com/group/swatuser" text="SWAT User Support Group" /></li>
					</ul>

					<div class="card mt-4">
						<div class="card-header">
							<h4 class="mb-0">Troubleshooting</h4>
						</div>
						<div class="card-body">
							<p>
								Trouble running SWAT Check? Please send the information below to the user group along with your error message.
							</p>
							<div>SWAT Check Version: {{ globals.version }}</div>
							<div>Platform: {{ globals.platform }}</div>
							<div>Project CMD Input: {{ globals.project_path }}</div>
							<div>Development Mode: {{ globals.dev_mode ? 'Yes' : 'No' }}</div>
						</div>
					</div>
				</div>

				<div v-else-if="!page.isReady" class="mx-auto text-center">
					<div class="alert alert-warning w-50 mx-auto lead" role="alert">
						Please load your model from the <a href="#" @click="setPageTab('setup')">setup</a> section.
					</div>
				</div>

				<div v-else-if="page.tab === 'hydrology'">
				
				</div>

				<div v-else-if="page.tab === 'sediment'">

				</div>

				<div v-else-if="page.tab === 'nitrogenCycle'">

				</div>

				<div v-else-if="page.tab === 'phosphorusCycle'">

				</div>

				<div v-else-if="page.tab === 'plantGrowth'">

				</div>

				<div v-else-if="page.tab === 'landscapeNutrientLosses'">

				</div>

				<div v-else-if="page.tab === 'landUseSummary'">

				</div>

				<div v-else-if="page.tab === 'instreamProcesses'">

				</div>

				<div v-else-if="page.tab === 'pointSources'">

				</div>

				<div v-else-if="page.tab === 'reservoirs'">

				</div>
			</div>
		</main>
	</div>
</template>

<style lang="scss">
	@import 'app.scss';
</style>
