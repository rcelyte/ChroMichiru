.SILENT:

include makefile.user

ChroMichiru.dll: Plugin.cs $(wildcard data/*)
	@echo "[csc $@]"
	csc -nologo -t:library -o+ -debug- Plugin.cs -res:ChroMichiru.angr -out:$@ -r:$(CMINSTALL)/ChroMapper_Data/Managed/Main.dll \
		-r:$(CMINSTALL)/ChroMapper_Data/Managed/UnityEngine.dll,$(CMINSTALL)/ChroMapper_Data/Managed/UnityEngine.CoreModule.dll \
		-r:$(CMINSTALL)/ChroMapper_Data/Managed/UnityEngine.UI.dll -r:$(CMINSTALL)/ChroMapper_Data/Managed/Unity.TextMeshPro.dll \
		-r:$(CMINSTALL)/ChroMapper_Data/Managed/UnityEngine.ImageConversionModule.dll \
		-r:$(CMINSTALL)/ChroMapper_Data/Managed/UnityEngine.AssetBundleModule.dll

$(CMINSTALL)/Plugins/ChroMichiru.dll: ChroMichiru.dll
	@echo "[cp $(notdir $@)]"
	mkdir -p "$(@D)"
	cp "$<" "$@"

install: $(CMINSTALL)/Plugins/ChroMichiru.dll

clean:
	@echo "[rm ChroMichiru.dll]"
	$(RM) ChroMichiru.dll

run: install
	cd $(CMINSTALL) && ./ChroMapper

.PHONY: install clean run
