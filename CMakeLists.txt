PROJECT (Main LANGUAGES CSharp)
CMAKE_MINIMUM_REQUIRED ( VERSION 3.12)
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "4.5.2")
add_executable (${PROJECT_NAME} Command.cs CommandType.cs GCode.cs Program.cs Tests.cs App.config Properties/AssemblyInfo.cs)

add_custom_command(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND nuget install OpenTK -version 2.0.0 -OutputDirectory ${CMAKE_BINARY_DIR}
	)

add_custom_command(TARGET ${PROJECT_NAME} 
	PRE_BUILD
	COMMAND nuget install CPTR302DantHardware -version 1.0.0 -OutputDirectory ${CMAKE_BINARY_DIR}
	)

	
set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DOTNET_REFERENCES  "System" "System.Drawing" "CPTR302DantHardware.1.0.0/lib/net452/CPTR302DantHardware.dll" "OpenTK.2.0.0/lib/net20/OpenTK.dll" "Firmware/Release/Firmware.dll")

set(CMAKE_CSharp_FLAGS "/langversion:7")

include(ExternalProject)
ExternalProject_Add(Firmware
                    PREFIX ${CMAKE_BINARY_DIR}
                    GIT_REPOSITORY "https://github.com/dantmw/3DPrinterFirmware.git"
                    PATCH_COMMAND ""
                    BINARY_DIR ${CMAKE_BINARY_DIR}/Firmware
                    SOURCE_DIR ${CMAKE_BINARY_DIR}/Firmware
                    INSTALL_DIR ${CMAKE_BINARY_DIR}/Firmware
					INSTALL_COMMAND ""
					)
					
add_dependencies(${PROJECT_NAME} Firmware)

