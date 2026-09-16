import * as THREE from
    "https://cdn.jsdelivr.net/npm/three@0.180.0/build/three.module.js";


// ======================================================
// CONTAINER
// ======================================================

const container =
    document.getElementById("three-container");

if (!container) {
    throw new Error("three-container not found");
}


// ======================================================
// SCENE
// ======================================================

const scene =
    new THREE.Scene();

scene.background =
    new THREE.Color(0x050505);


// ======================================================
// CAMERA
// ======================================================

const camera =
    new THREE.PerspectiveCamera(
        60,
        container.clientWidth /
        container.clientHeight,
        0.1,
        100
    );

camera.position.set(
    0,
    0,
    7
);


// ======================================================
// RENDERER
// ======================================================

const renderer =
    new THREE.WebGLRenderer({
        antialias: true
    });

renderer.setPixelRatio(
    Math.min(
        window.devicePixelRatio,
        2
    )
);

renderer.setSize(
    container.clientWidth,
    container.clientHeight
);

container.appendChild(
    renderer.domElement
);


// ======================================================
// LIGHT
// ======================================================

const ambientLight =
    new THREE.AmbientLight(
        0xffffff,
        2
    );

scene.add(
    ambientLight
);

const pointLight =
    new THREE.PointLight(
        0xffffff,
        100,
        30
    );

pointLight.position.set(
    3,
    3,
    5
);

scene.add(
    pointLight
);


// ======================================================
// VINYL GROUP
// ======================================================

const vinylGroup =
    new THREE.Group();

scene.add(
    vinylGroup
);


// ======================================================
// VINYL RECORD
// ======================================================

const vinylGeometry =
    new THREE.CylinderGeometry(
        2.2,
        2.2,
        0.12,
        128
    );

const vinylMaterial =
    new THREE.MeshStandardMaterial({
        color: 0x111111,
        roughness: 0.3,
        metalness: 0.15
    });

const vinyl =
    new THREE.Mesh(
        vinylGeometry,
        vinylMaterial
    );


// Cylinder normally stands vertically.
// Rotate it so the record faces the camera.
vinyl.rotation.x =
    Math.PI / 2;

vinylGroup.add(
    vinyl
);


// ======================================================
// CENTER LABEL
// ======================================================

const labelGeometry =
    new THREE.CylinderGeometry(
        0.65,
        0.65,
        0.14,
        64
    );

const labelMaterial =
    new THREE.MeshStandardMaterial({
        color: 0x444444,
        roughness: 0.5
    });

const label =
    new THREE.Mesh(
        labelGeometry,
        labelMaterial
    );

label.rotation.x =
    Math.PI / 2;

label.position.z =
    0.08;

vinylGroup.add(
    label
);


// ======================================================
// CENTER HOLE
// ======================================================

const holeGeometry =
    new THREE.CylinderGeometry(
        0.08,
        0.08,
        0.18,
        32
    );

const holeMaterial =
    new THREE.MeshStandardMaterial({
        color: 0x050505
    });

const hole =
    new THREE.Mesh(
        holeGeometry,
        holeMaterial
    );

hole.rotation.x =
    Math.PI / 2;

hole.position.z =
    0.12;

vinylGroup.add(
    hole
);


// ======================================================
// VINYL GROOVES
// ======================================================

for (
    let radius = 0.85;
    radius < 2.1;
    radius += 0.08
) {

    const curve =
        new THREE.EllipseCurve(
            0,
            0,
            radius,
            radius,
            0,
            Math.PI * 2,
            false,
            0
        );

    const points =
        curve.getPoints(128);

    const geometry =
        new THREE.BufferGeometry()
            .setFromPoints(
                points
            );

    const material =
        new THREE.LineBasicMaterial({
            color: 0x333333
        });

    const groove =
        new THREE.LineLoop(
            geometry,
            material
        );

    groove.position.z =
        0.075;

    vinylGroup.add(
        groove
    );
}


// ======================================================
// PARTICLES
// ======================================================

const particleCount =
    800;

const particlePositions =
    new Float32Array(
        particleCount * 3
    );

for (
    let i = 0;
    i < particleCount;
    i++
) {

    const i3 =
        i * 3;

    particlePositions[i3] =
        (Math.random() - 0.5) * 15;

    particlePositions[i3 + 1] =
        (Math.random() - 0.5) * 10;

    particlePositions[i3 + 2] =
        (Math.random() - 0.5) * 10;
}

const particleGeometry =
    new THREE.BufferGeometry();

particleGeometry.setAttribute(
    "position",
    new THREE.BufferAttribute(
        particlePositions,
        3
    )
);

const particleMaterial =
    new THREE.PointsMaterial({
        color: 0xffffff,
        size: 0.025
    });

const particles =
    new THREE.Points(
        particleGeometry,
        particleMaterial
    );

scene.add(
    particles
);


// ======================================================
// MOUSE
// ======================================================

const mouse = {
    x: 0,
    y: 0
};

container.addEventListener(
    "mousemove",
    function (event) {

        const rect =
            container.getBoundingClientRect();

        mouse.x =
            ((event.clientX - rect.left) /
                rect.width) * 2 - 1;

        mouse.y =
            -((event.clientY - rect.top) /
                rect.height) * 2 + 1;
    }
);


// ======================================================
// ANIMATION
// ======================================================

const clock =
    new THREE.Clock();

function animate() {

    requestAnimationFrame(
        animate
    );

    const elapsed =
        clock.getElapsedTime();


    // Vinyl spinning
    vinylGroup.rotation.z =
        elapsed * 0.8;


    // Floating up and down
    vinylGroup.position.y =
        Math.sin(
            elapsed * 1.2
        ) * 0.15;


    // Mouse movement
    vinylGroup.rotation.x +=
        (
            mouse.y * 0.25 -
            vinylGroup.rotation.x
        ) * 0.03;

    vinylGroup.rotation.y +=
        (
            mouse.x * 0.25 -
            vinylGroup.rotation.y
        ) * 0.03;


    // Particles
    particles.rotation.y =
        elapsed * 0.015;


    // Render
    renderer.render(
        scene,
        camera
    );
}

animate();


// ======================================================
// RESIZE
// ======================================================

window.addEventListener(
    "resize",
    function () {

        const width =
            container.clientWidth;

        const height =
            container.clientHeight;

        camera.aspect =
            width / height;

        camera.updateProjectionMatrix();

        renderer.setSize(
            width,
            height
        );
    }
);