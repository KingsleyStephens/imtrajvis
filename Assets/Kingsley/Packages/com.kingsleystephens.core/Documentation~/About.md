***About***

# ExecutionSystem

A system to update objects. Designed as a replacement for Unity's standard Monobehaviour updating pattern.
Allows update cycles to be based on a time interval and for objects to be given execution priority.
Derive objects to be updated from Updateable and add the Singleton ExecutionManager to a game object in the scene.

# Logging

Console logging by default with the option to also write to a log file. Allows for the creation of custom log channels that
can write raw logs entries without timestamps - useful for telemetry or other formatted output. Log levels can be set to 
control what log outputs appear.
