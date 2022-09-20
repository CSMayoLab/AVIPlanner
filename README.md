# AVIPlanner

Eclipse Scripting API based Automatic planner for Head and Neck VMAT.

Create RT plan based on historical manual planning experiences (summarized into pre-calculated Json files).

# Author

University of Michigan - Department of Radiation Oncology

Chuck Mayo - cmayo@med.umich.edu

John Yao - yuayao@med.umich.edu

# Disclaimer

This code should only be used in research and development settings.

# Project structure

- AutoPlan_HN is the main project which produces AutoPlan_HN exe file.

- AP_lib and AnalysisLibrary2 produce lib dll files.

- Folder *Pre_calculated_data* contains pre-calculated json files that summarized historical experiences at University of Michigan between 2016-2019.

# Prerequisite

1. Varian Eclipse Treatment planning system (v15 or v16 with writable script enabled)

2. Visual Studio needed to compile the source code




# Configuration

1. App.config (which will be compiled into name_of_the_exe.config)

   Configurations inside this file need to be customized according to the local environment.
  
   ### Must change:
   
      `Precalculated_JSON_files_dir` needs to point to the Pre_calculated_data directory

      `MachineIDs` is the list of the names/ID of available treatment machines

      `PhotonVMATOptimization` and `PhotonVolumeDose` should be the actual name of the available calculation algorithms

   ### Optional change
      
      Log file directory, Jaw width limit, NTO parameters, Priority mapping... etc.

2. constraints_config.json

   This file configs constraints that are applied to PTV and OARs.



