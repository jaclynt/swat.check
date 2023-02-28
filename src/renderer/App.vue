<script setup lang="ts">

</script>

<template>
	<div id="app">
		<div class="container">
			<h1>SWAT Check</h1>

			<p>
				SWAT Check reads model output from a SWAT project and performs many simple checks to identify
				potential model problems. The intended purpose of this program is to identify model problems early 
				in the modeling process. Hidden model problems often result in the need to recalibrate or regenerate a model, 
				resulting in an avoidable waste of time. This program is designed to compare a variety of SWAT outputs to 
				nominal ranges based on the judgment of model developers. A warning does not necessarily indicate a problem; 
				the purpose is to bring attention to unusual predictions. This software also provides a visual representation 
				of various model outputs to aid novice users.
			</p>
		</div>
	</div>
</template>

<style lang="scss">
	@import 'app.scss';
</style>
