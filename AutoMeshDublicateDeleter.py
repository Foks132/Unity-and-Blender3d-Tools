import bpy

objects = bpy.context.selected_objects

for item in objects:
    if item.data.name.find('.') != -1:
        text = item.data.name.partition('.')
        item_name_no_point = text[0]
        print(item_name_no_point)
        huy=0
        
        for i in range(0, len(bpy.data.meshes)):
            if bpy.data.meshes[i].name == item_name_no_point:
                huy = i
            
        item.data = bpy.data.meshes[huy]
        
bpy.ops.outliner.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=False)