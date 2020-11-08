SCRIPT_NAME random_vehicle_colors

NATIVE PROC WAIT(INT ms)
NATIVE FUNC INT GET_GAME_TIMER()
NATIVE PROC BEGIN_TEXT_COMMAND_DISPLAY_TEXT(STRING text)
NATIVE PROC END_TEXT_COMMAND_DISPLAY_TEXT(FLOAT x, FLOAT y, INT p2)
NATIVE PROC ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(STRING text)
NATIVE PROC ADD_TEXT_COMPONENT_INTEGER(INT value)
NATIVE PROC ADD_TEXT_COMPONENT_FLOAT(FLOAT value, INT decimalPlaces)
NATIVE FUNC FLOAT TIMESTEP()
NATIVE FUNC BOOL IS_CONTROL_PRESSED(INT padIndex, INT control)
NATIVE FUNC FLOAT VMAG(VEC3 v)
NATIVE FUNC FLOAT VMAG2(VEC3 v)
NATIVE FUNC FLOAT VDIST(VEC3 v1, VEC3 v2)
NATIVE FUNC FLOAT VDIST2(VEC3 v1, VEC3 v2)
NATIVE FUNC VEC3 GET_GAMEPLAY_CAM_COORD()
NATIVE PROC DELETE_PED(PED_INDEX& handle)
NATIVE FUNC PED_INDEX PLAYER_PED_ID()
NATIVE FUNC VEHICLE_INDEX GET_VEHICLE_PED_IS_IN(PED_INDEX ped, BOOL includeLastVehicle)
NATIVE FUNC BOOL DOES_ENTITY_EXIST(ENTITY_INDEX entity)
NATIVE FUNC INT GET_RANDOM_INT_IN_RANGE(INT startRange, INT endRange)
NATIVE PROC SET_VEHICLE_CUSTOM_PRIMARY_COLOUR(VEHICLE_INDEX vehicle, INT r, INT g, INT b)

INT result

PROC MAIN()
    WHILE TRUE
        WAIT(100)

        VEHICLE_INDEX veh = GET_VEHICLE_PED_IS_IN(PLAYER_PED_ID(), TRUE)

        IF DOES_ENTITY_EXIST(veh.base)
            SET_VEHICLE_CUSTOM_PRIMARY_COLOUR(veh, GET_RANDOM_INT_IN_RANGE(0, 255), GET_RANDOM_INT_IN_RANGE(0, 255), GET_RANDOM_INT_IN_RANGE(0, 255))
        ENDIF

        SOMETHING(5)

    ENDWHILE

ENDPROC

PROC SOMETHING(INT a)
    result = GET_RANDOM_INT_IN_RANGE(a, a + 10)
ENDPROC
