let cubeCaseMap = [
  { index: 0, indices: [], alternative: null },
  { index: 0b00010000, indices: [3, 11, 2], alternative: null },
  { index: 0b00110000, indices: [3, 11, 1,  1, 11, 10], alternative: null },

  { index: 0b00010010, indices: [3, 11, 2,  5, 10, 6], alternative: [6, 11, 3,  5, 6, 3,  5, 3, 10,  10, 3, 2] },
  { index: 0b00010100, indices: [3, 11, 2,  4, 9, 5], alternative: null },
  { index: 0b11100000, indices: [9, 8, 10,  10, 8, 3,  3, 8, 10], alternative: null },

  { index: 0b00110100, indices: [3, 11, 1,  1, 11, 10,  4, 9, 5], alternative: [5, 10, 11,  5, 11, 4,  4, 11, 9,  9, 11, 3,  9, 3, 1] },
  { index: 0b00100101, indices: [4, 9, 5,  6, 11, 7,  10, 1, 2], alternative: [7, 11, 4,  4, 11, 2,  2, 9, 4,  9, 2, 1,  6, 5, 10] },
  { index: 0b11110000, indices: [1, 8, 11,  11, 10, 9], alternative: null },

  { index: 0b11011000, indices: [7, 11, 4,  4, 11, 2,  2, 9, 4,  2, 1, 9], alternative: null },
  { index: 0b01010101, indices: [7, 6, 3,  3, 7, 2,  4, 0, 5,  5, 0, 1], alternative: [4, 7, 3,  3, 0, 4,  6, 5, 2,  2, 5, 1] },
  { index: 0b11010100, indices: [5, 4, 8,  8, 2, 5,  5, 2, 1,  8, 11, 2], alternative: null },

  { index: 0b11100001, indices: [6, 11, 7,  9, 8, 10,  10, 8, 3,  10, 3, 2], alternative: [6, 7, 8,  6, 8, 10,  10, 8, 9,  11, 3, 2] },
  { index: 0b01011010, indices: [7, 8, 4,  6, 5, 10,  3, 11, 2,  9, 0, 1], alternative: [7, 8, 4,  6, 5, 10,  11, 2, 3,  8, 0, 1] },
  { index: 0b11101000, indices: [4, 7, 9,  9, 7, 2,  9, 2, 10,  7, 3, 2], alternative: null }
];

const rotX = index => (0x40&index)>>1|(0x02&index)<<1|(0x0D&index)<<4|(0xB0&index)>>4;
const rotY = index => (0x77&index)<<1|(0x88&index)>>3;
const rotZ = index => (0x09&index)<<4|(0x60&index)>>4|(0x14&index)<<1|(0x82&index)>>1;

const edgeRotX = [
  2, 10, 6, 11, 0, 9, 4, 8, 3, 1, 5, 7
];

const edgeRotY = [
  3, 0, 1, 2, 7, 4, 5, 6, 11, 8, 9, 10
];

const edgeRotZ = [
  9, 5, 10, 1, 6, 7, 11, 3, 0, 4, 6, 2
];

const rotIndices = (indices, rotation) => indices.map(i => rotation[i]);

const tris = [];

const triagIndices = []; 

cubeCaseMap.forEach(({ index, indices, alternative }) => {
  let workingIndex = index;
  for(let z = 0; z < 4; z++) {
    for(let y = 0; y < 4; y++) {
      for(let x = 0; x < 4; x++) {
        tris[workingIndex] = indices.length/3;
        triagIndices[workingIndex] = indices;
        workingIndex = rotX(workingIndex);
	indices = rotIndices(indices, edgeRotX);
      }
      workingIndex = rotY(rotX(workingIndex));
      indices = rotIndices(rotIndices(indices, edgeRotX), edgeRotY);
    }
    workingIndex = rotZ(rotY(rotX(workingIndex)));
    indices = rotIndices(rotIndices(rotIndices(indices, edgeRotX), edgeRotY), edgeRotZ);
  }

  workingIndex = index^0xFF;
  indices = alternative !== null ? alternative : indices;
  for(let z = 0; z < 4; z++) {
    for(let y = 0; y < 4; y++) {
      for(let x = 0; x < 4; x++) {
        tris[workingIndex] = indices.length/3;
	if (tris[workingIndex] === undefined) {
	  console.log(x, y, z);
	}
        triagIndices[workingIndex] = indices;
        workingIndex = rotX(workingIndex);
	indices = rotIndices(indices, edgeRotX);
      }
      workingIndex = rotY(rotX(workingIndex));
      indices = rotIndices(rotIndices(indices, edgeRotX), edgeRotY);
    }
    workingIndex = rotZ(rotY(rotX(workingIndex)));
    indices = rotIndices(rotIndices(rotIndices(indices, edgeRotX), edgeRotY), edgeRotZ);
  }
});


const formatJoin = arr => {
  let erg = '';
  for (let i = 0; i < arr.length; i+=16) {
    for(let j = 0; j < 16; j++) {
      if (i+j >= arr.length) {
	break;
      }
      erg += arr[i+j] + ', ';
    }
    erg += '\n';
  }
  return erg;
}

const trisAccumulated = tris.reduce((acc, cur) => {
  acc.push(acc[acc.length-1] ? acc[acc.length-1] + cur : cur);
  return acc;
}, []);

const triagIndicesString = formatJoin(triagIndices.reduce((acc, cur) => acc.concat(cur)));

console.log(formatJoin(tris));
console.log(formatJoin(trisAccumulated));
console.log(triagIndicesString);

