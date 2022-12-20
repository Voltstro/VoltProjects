import {VANTA} from 'vanta/src/_base.js'
import 'vanta/src/vanta.waves.js'

async function mountWaves() {
    
    const THREE = await import("three/src/Three.js");
    
    VANTA.WAVES({
        el: "#hero",
        THREE: THREE,
        mouseControls: false,
        touchControls: false,
        gyroControls: false,
        minHeight: 200.00,
        minWidth: 200.00,
        scale: 1.00,
        scaleMobile: 1.00,
        color: 0x3948,
        shininess: 19.00,
        waveHeight: 20.50,
        waveSpeed: 0.20,
        zoom: 0.95
    })
}

mountWaves().then(() => console.log("Loaded hero waves."));