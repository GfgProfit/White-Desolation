const http = require('http');
const crypto = require('crypto');
const fs = require('fs/promises');
const path = require('path');

const PORT = Number(process.env.PORT || 3000);
const USERS_DIRECTORY = path.resolve(process.env.USERS_DIRECTORY || path.join(__dirname, 'data', 'users'));
const MAX_BODY_BYTES = 10 * 1024 * 1024;
const SAVE_ENCRYPTION_ALGORITHM = 'aes-256-gcm';
const SAVE_ENCRYPTION_VERSION = 1;
const SAVE_ENCRYPTION_KEY = createSaveEncryptionKey();

function sendJson(response, statusCode, payload) {
  response.writeHead(statusCode, {
    'Content-Type': 'application/json; charset=utf-8',
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET,POST,PUT,DELETE,OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type'
  });

  response.end(JSON.stringify(payload));
}

function sendNoContent(response) {
  response.writeHead(204, {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET,POST,PUT,DELETE,OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type'
  });

  response.end();
}

function normalizeNickname(value) {
  if (typeof value !== 'string') {
    return '';
  }

  return value
    .trim()
    .normalize('NFKC')
    .replace(/[<>:"/\\|?*\x00-\x1F]/g, '_')
    .replace(/\.+/g, '_')
    .slice(0, 32);
}

function sanitizeSlotName(value) {
  if (typeof value !== 'string') {
    return 'slot_0';
  }

  const sanitized = value
    .trim()
    .replace(/[<>:"/\\|?*\x00-\x1F]/g, '_')
    .replace(/\.+/g, '_')
    .slice(0, 64);

  return sanitized || 'slot_0';
}

function getUserPath(nickname) {
  return path.join(USERS_DIRECTORY, `${nickname}.json`);
}

async function ensureUsersDirectory() {
  await fs.mkdir(USERS_DIRECTORY, { recursive: true });
}

async function readRequestBody(request) {
  return new Promise((resolve, reject) => {
    let body = '';

    request.setEncoding('utf8');
    request.on('data', chunk => {
      body += chunk;

      if (Buffer.byteLength(body, 'utf8') > MAX_BODY_BYTES) {
        reject(new Error('Request body is too large.'));
        request.destroy();
      }
    });

    request.on('end', () => {
      if (!body) {
        resolve(null);
        return;
      }

      try {
        resolve(JSON.parse(body));
      } catch {
        reject(new Error('Request body must be valid JSON.'));
      }
    });

    request.on('error', reject);
  });
}

async function readUser(nickname) {
  try {
    const json = await fs.readFile(getUserPath(nickname), 'utf8');
    const user = JSON.parse(json);
    user.slots = user.slots || {};
    return user;
  } catch (error) {
    if (error.code === 'ENOENT') {
      return null;
    }

    throw error;
  }
}

async function writeUser(user) {
  await ensureUsersDirectory();

  const userPath = getUserPath(user.nickname);
  const tempPath = `${userPath}.tmp`;
  const json = `${JSON.stringify(user, null, 2)}\n`;

  await fs.writeFile(tempPath, json, 'utf8');
  await fs.rename(tempPath, userPath);
}

function hasAnySave(user) {
  return Boolean(user && user.slots && Object.keys(user.slots).some(slot => user.slots[slot]));
}

function createSaveEncryptionKey() {
  const secret = process.env.SAVE_ENCRYPTION_KEY || 'white-desolation-local-save-key';
  return crypto.createHash('sha256').update(secret, 'utf8').digest();
}

function encryptSaveSlot(saveData) {
  const iv = crypto.randomBytes(12);
  const cipher = crypto.createCipheriv(SAVE_ENCRYPTION_ALGORITHM, SAVE_ENCRYPTION_KEY, iv);
  const json = JSON.stringify(saveData);
  const encrypted = Buffer.concat([cipher.update(json, 'utf8'), cipher.final()]);
  const authTag = cipher.getAuthTag();

  return {
    encrypted: true,
    version: SAVE_ENCRYPTION_VERSION,
    algorithm: SAVE_ENCRYPTION_ALGORITHM,
    iv: iv.toString('base64'),
    authTag: authTag.toString('base64'),
    data: encrypted.toString('base64')
  };
}

function decryptSaveSlot(slotData) {
  if (!isEncryptedSaveSlot(slotData)) {
    return slotData;
  }

  if (slotData.version !== SAVE_ENCRYPTION_VERSION || slotData.algorithm !== SAVE_ENCRYPTION_ALGORITHM) {
    throw new Error('Unsupported encrypted save slot format.');
  }

  const iv = Buffer.from(slotData.iv, 'base64');
  const authTag = Buffer.from(slotData.authTag, 'base64');
  const encrypted = Buffer.from(slotData.data, 'base64');
  const decipher = crypto.createDecipheriv(SAVE_ENCRYPTION_ALGORITHM, SAVE_ENCRYPTION_KEY, iv);
  decipher.setAuthTag(authTag);

  const decrypted = Buffer.concat([decipher.update(encrypted), decipher.final()]).toString('utf8');
  return JSON.parse(decrypted);
}

function isEncryptedSaveSlot(slotData) {
  return Boolean(slotData && typeof slotData === 'object' && slotData.encrypted === true);
}

async function getOrCreateUser(nickname) {
  await ensureUsersDirectory();

  const existingUser = await readUser(nickname);

  if (existingUser) {
    return { user: existingUser, existed: true };
  }

  const now = new Date().toISOString();
  const user = {
    nickname,
    createdAt: now,
    updatedAt: now,
    slots: {}
  };

  await writeUser(user);
  return { user, existed: false };
}

function getRouteParts(requestUrl) {
  const url = new URL(requestUrl, `http://localhost:${PORT}`);
  return url.pathname.split('/').filter(Boolean).map(part => decodeURIComponent(part));
}

function buildUserResponse(user, existed) {
  return {
    nickname: user.nickname,
    existed,
    hasSave: hasAnySave(user)
  };
}

async function handleAuthorize(request, response) {
  const body = await readRequestBody(request);
  const nickname = normalizeNickname(body && body.nickname);

  if (!nickname) {
    sendJson(response, 400, { error: 'Nickname is required.' });
    return;
  }

  const { user, existed } = await getOrCreateUser(nickname);
  sendJson(response, existed ? 200 : 201, buildUserResponse(user, existed));
}

async function handleGetUser(parts, response) {
  const nickname = normalizeNickname(parts[2]);

  if (!nickname) {
    sendJson(response, 400, { error: 'Nickname is required.' });
    return;
  }

  const user = await readUser(nickname);

  if (!user) {
    sendJson(response, 404, { error: 'User not found.' });
    return;
  }

  sendJson(response, 200, buildUserResponse(user, true));
}

async function handleSaveExists(parts, response) {
  const nickname = normalizeNickname(parts[2]);
  const slotName = sanitizeSlotName(parts[4]);
  const user = await readUser(nickname);

  sendJson(response, 200, {
    exists: Boolean(user && user.slots && user.slots[slotName])
  });
}

async function handleLoadSave(parts, response) {
  const nickname = normalizeNickname(parts[2]);
  const slotName = sanitizeSlotName(parts[4]);
  const user = await readUser(nickname);

  if (!user || !user.slots || !user.slots[slotName]) {
    sendJson(response, 404, { error: 'Save not found.' });
    return;
  }

  try {
    sendJson(response, 200, decryptSaveSlot(user.slots[slotName]));
  } catch (error) {
    console.error(error);
    sendJson(response, 500, { error: 'Save slot cannot be decrypted.' });
  }
}

async function handleWriteSave(request, parts, response) {
  const nickname = normalizeNickname(parts[2]);
  const slotName = sanitizeSlotName(parts[4]);
  const body = await readRequestBody(request);

  if (!nickname) {
    sendJson(response, 400, { error: 'Nickname is required.' });
    return;
  }

  if (!body || typeof body !== 'object') {
    sendJson(response, 400, { error: 'Save payload is required.' });
    return;
  }

  const { user } = await getOrCreateUser(nickname);
  user.slots = user.slots || {};
  user.slots[slotName] = encryptSaveSlot(body);
  user.updatedAt = new Date().toISOString();

  await writeUser(user);

  sendJson(response, 200, {
    nickname: user.nickname,
    slotName,
    hasSave: true
  });
}

async function handleDeleteSave(parts, response) {
  const nickname = normalizeNickname(parts[2]);
  const slotName = sanitizeSlotName(parts[4]);
  const user = await readUser(nickname);

  if (user && user.slots && user.slots[slotName]) {
    delete user.slots[slotName];
    user.updatedAt = new Date().toISOString();
    await writeUser(user);
  }

  sendJson(response, 200, {
    nickname,
    slotName,
    hasSave: hasAnySave(user)
  });
}

async function handleRequest(request, response) {
  if (request.method === 'OPTIONS') {
    sendNoContent(response);
    return;
  }

  const parts = getRouteParts(request.url);

  if (request.method === 'POST' && parts.length === 2 && parts[0] === 'api' && parts[1] === 'users') {
    await handleAuthorize(request, response);
    return;
  }

  if (request.method === 'GET' && parts.length === 3 && parts[0] === 'api' && parts[1] === 'users') {
    await handleGetUser(parts, response);
    return;
  }

  const isSaveRoute = parts.length >= 5 && parts[0] === 'api' && parts[1] === 'users' && parts[3] === 'saves';

  if (isSaveRoute && request.method === 'GET' && parts.length === 6 && parts[5] === 'exists') {
    await handleSaveExists(parts, response);
    return;
  }

  if (isSaveRoute && request.method === 'GET' && parts.length === 5) {
    await handleLoadSave(parts, response);
    return;
  }

  if (isSaveRoute && request.method === 'PUT' && parts.length === 5) {
    await handleWriteSave(request, parts, response);
    return;
  }

  if (isSaveRoute && request.method === 'DELETE' && parts.length === 5) {
    await handleDeleteSave(parts, response);
    return;
  }

  sendJson(response, 404, { error: 'Endpoint not found.' });
}

const server = http.createServer((request, response) => {
  handleRequest(request, response).catch(error => {
    console.error(error);
    sendJson(response, 500, { error: error.message || 'Internal server error.' });
  });
});

server.listen(PORT, () => {
  console.log(`White Desolation server is running at http://localhost:${PORT}`);
  console.log(`User data directory: ${USERS_DIRECTORY}`);
});
