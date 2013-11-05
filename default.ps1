#-------------------------------------------------------------------------------  
# basic build file Powershell script for psake
#-------------------------------------------------------------------------------  
properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\lib"
  $source_dir = "$base_dir\src"
  $tests_dir = "$base_dir\tests"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\MicroORM.sln" 
  $test_lib = "MicroORM.Tests.dll"
  $version = "1.1.0.0"
  $tools_dir = "$base_dir\tools"
  $release_dir = "$base_dir\release"
} 

#-------------------------------------------------------------------------------  
# entry task to start the build script
#-------------------------------------------------------------------------------  
task default -depends test

#-------------------------------------------------------------------------------  
# clean the "build" directory and make ready for the build actions
#-------------------------------------------------------------------------------  
task clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
} 

#-------------------------------------------------------------------------------  
# initializes all of the directories and other objects in preparation for the 
# build process
#-------------------------------------------------------------------------------  
task init -depends clean { 
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
} 

task compile -depends init { 
  copy-item "$lib_dir\*" $build_dir
  copy-item "$tools_dir\*" $build_dir
  exec { msbuild /p:OutDir="$buildartifacts_dir" "$sln_file" }
} 

task test -depends compile {
  $old = pwd
  cd $build_dir
  
  # using .NET 4.0 runner for xunit:
  $xunit = "$tools_dir\xUnit\xunit.console.clr4.x86.exe"
  .$xunit "$buildartifacts_dir\$test_lib"
  cd $old		
}


