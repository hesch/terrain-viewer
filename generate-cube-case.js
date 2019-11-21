let cubeCaseMap = [
  { index: 0, numTris: 0 },
  { index: 0b00001000, numTris: 1 },
  { index: 0b00001100, numTris: 2 },

  { index: 0b01001000, numTris: 2 },
  { index: 0b00101000, numTris: 2 },
  { index: 0b00000111, numTris: 3 },

  { index: 0b00101100, numTris: 3 },
  { index: 0b10100100, numTris: 3 },
  { index: 0b00001111, numTris: 2 },

  { index: 0b00011011, numTris: 4 },
  { index: 0b10101010, numTris: 4 },
  { index: 0b00101011, numTris: 4 },

  { index: 0b10000111, numTris: 4 },
  { index: 0b01011010, numTris: 4 },
  { index: 0b00010111, numTris: 4 }
];

const rotX = index => (0xC0&index)>>2|(0x03&index)<<2|(0x0C&index)<<4|(0x30&index)>>4;
const rotY = index => (0x77&index)<<1|(0x88&index)>>3;
const rotZ = index => (0x09&index)<<4|(0x60&index)>>4|(0x14&index)<<1|(0x82&index)>>1;

const tris = [];

cubeCaseMap.forEach(({ index, numTris }) => {
  let workingIndex = index;
  for(let z = 0; z < 4; z++) {
    for(let y = 0; y < 4; y++) {
      for(let x = 0; x < 4; x++) {
        tris[workingIndex] = numTris;
        workingIndex = rotX(workingIndex);
      }
      workingIndex = rotY(rotX(workingIndex));
    }
    workingIndex = rotZ(rotY(rotX(workingIndex)));
  }

  workingIndex = index^0xFF;
  for(let z = 0; z < 4; z++) {
    for(let y = 0; y < 4; y++) {
      for(let x = 0; x < 4; x++) {
        tris[workingIndex] = numTris;
        workingIndex = rotX(workingIndex);
      }
      workingIndex = rotY(rotX(workingIndex));
    }
    workingIndex = rotZ(rotY(rotX(workingIndex)));
  }
});

console.log(tris.join(','));

