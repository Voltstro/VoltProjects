import { DynamicDrawUsage } from "three/src/constants"
import { WebGLRenderer } from "three/src/renderers/WebGLRenderer"
import { BufferGeometry } from "three/src/core/BufferGeometry"
import { Scene } from "three/src/scenes/Scene"
import { PerspectiveCamera } from "three/src/cameras/PerspectiveCamera"
import { AmbientLight } from "three/src/lights/AmbientLight"
import { PointLight } from "three/src/lights/PointLight"
import { Mesh } from "three/src/objects/Mesh"
import { MeshPhongMaterial } from "three/src/materials/MeshPhongMaterial"

//We doing three like this so we only have what we need
export { DynamicDrawUsage, 
    WebGLRenderer, BufferGeometry, 
    Scene,
    PerspectiveCamera,
    AmbientLight, PointLight,
    Mesh, MeshPhongMaterial
};